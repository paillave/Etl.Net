using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.System.TraceContents;

namespace Paillave.Etl.Core.System.Streams
{
    public class Stream<T> : IStream<T>
    {
        public Stream(ITracer tracer, IExecutionContext executionContext, string sourceOutputName, IPushObservable<T> observable)
        {
            this.ExecutionContext = executionContext;

            observable = observable.CompletesOnException(e => tracer.Trace(new UnhandledExceptionStreamTraceContent(sourceOutputName, e)));
            //observable = executionContext.StopIfContextStops(observable);
            if (tracer != null)
            {
                PushObservable.Merge<ITraceContent>(
                    observable.Count().Map(count => new CounterSummaryStreamTraceContent(sourceOutputName, count)),
                    observable.Map((e, i) => new RowProcessStreamTraceContent(sourceOutputName, i + 1, e))
                ).Subscribe(tracer.Trace);
            }
            this.Observable = observable.TakeUntil(executionContext.TraceEvents.Filter(i => i.Content.Level == TraceLevel.Error));
        }

        public IPushObservable<T> Observable { get; private set; }

        public IExecutionContext ExecutionContext { get; }
    }
}
