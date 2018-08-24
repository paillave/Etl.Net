using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.TraceContents;

namespace Paillave.Etl.Core.Streams
{
    public class Stream<T> : IStream<T>
    {
        public Stream(ITracer tracer, IExecutionContext executionContext,string sourceNodeName, string name, IPushObservable<T> observable)
        {
            this.Name = name;
            this.SourceNodeName = sourceNodeName;
            this.ExecutionContext = executionContext;

            this.Observable = observable
                .CompletesOnException(e => tracer.Trace(new UnhandledExceptionStreamTraceContent(name, e)))
                .TakeUntil(executionContext.TraceEvents.Filter(i => i.Content.Level == TraceLevel.Error));

            if (tracer != null)
            {
                PushObservable.Merge<ITraceContent>(
                    this.Observable.Count().Map(count => new CounterSummaryStreamTraceContent(name, count)),
                    this.Observable.Map((e, i) => new RowProcessStreamTraceContent(name, i + 1, e))
                ).Subscribe(tracer.Trace);
            }
        }

        public IPushObservable<T> Observable { get; }

        public IExecutionContext ExecutionContext { get; }

        public string Name { get; }

        public string SourceNodeName { get; }
    }
}
