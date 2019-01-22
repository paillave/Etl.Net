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
        protected ITraceMapper TraceMapper { get; }
        public Stream(ITraceMapper traceMapper, IExecutionContext executionContext, string sourceNodeName, IPushObservable<T> observable)
        {
            this.TraceMapper = traceMapper;
            this.SourceNodeName = sourceNodeName;
            this.ExecutionContext = executionContext;

            this.Observable = observable.TakeUntil(executionContext.StopProcessEvent);

            if (traceMapper != null)
            {
                this.TraceObservable =
                    PushObservable.Merge<ITraceContent>(
                        this.Observable.ExceptionsToObservable().Map(e => new UnhandledExceptionStreamTraceContent(e)),
                        this.Observable.Count().Map(count => new CounterSummaryStreamTraceContent(count)),
                        this.Observable.Map((e, i) => new RowProcessStreamTraceContent(i + 1, e))
                    ).Map(i => traceMapper.MapToTrace(i));
            }
        }

        public IPushObservable<T> Observable { get; }

        public IExecutionContext ExecutionContext { get; }

        public string SourceNodeName { get; }

        public IPushObservable<TraceEvent> TraceObservable { get; }

        public virtual object GetMatchingStream(IPushObservable<T> observable)
        {
            return new Stream<T>(this.TraceMapper, this.ExecutionContext, this.SourceNodeName, observable);
        }

        public virtual object GetMatchingStream(ITraceMapper tracer, IExecutionContext executionContext, string name, object observable)
        {
            return new Stream<T>(tracer, executionContext, name, (IPushObservable<T>)observable);
        }
    }
}
