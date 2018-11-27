using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.Core.TraceContents;

namespace Paillave.Etl.Core.Streams
{
    public class Stream<T> : IStream<T>
    {
        protected ITracer Tracer { get; }
        public Stream(ITracer tracer, IExecutionContext executionContext, string sourceNodeName, IPushObservable<T> observable)
        {
            this.Tracer = tracer;
            this.SourceNodeName = sourceNodeName;
            this.ExecutionContext = executionContext;

            this.Observable = observable
                .CompletesOnException(e => tracer.Trace(new UnhandledExceptionStreamTraceContent(e)))
                .TakeUntil(executionContext.StopProcessEvents);

            if (tracer != null)
            {
                PushObservable.Merge<ITraceContent>(
                    this.Observable.Count().Map(count => new CounterSummaryStreamTraceContent(count)),
                    this.Observable.Map((e, i) => new RowProcessStreamTraceContent(i + 1, e))
                ).Subscribe(tracer.Trace);
            }
        }

        public IPushObservable<T> Observable { get; }

        public IExecutionContext ExecutionContext { get; }

        public string SourceNodeName { get; }

        public virtual object GetMatchingStream(IPushObservable<T> observable)
        {
            return new Stream<T>(this.Tracer, this.ExecutionContext, this.SourceNodeName, observable);
        }

        public virtual object GetMatchingStream(ITracer tracer, IExecutionContext executionContext, string name, object observable)
        {
            return new Stream<T>(tracer, executionContext, name, (IPushObservable<T>)observable);
        }
    }
}
