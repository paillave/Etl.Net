using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.Core.TraceContents;
using System.Threading;

namespace Paillave.Etl.Core.Streams
{
    public class Stream<T> : IStream<T>
    {
        private class RowTracer
        {
            private Stopwatch _stopwatch = new Stopwatch();
            private int _count = 0;
            public RowProcessStreamTraceContent ProcessRow(T row, int rowNumber)
            {
                Interlocked.Increment(ref _count);
                return new RowProcessStreamTraceContent(rowNumber + 1, GetAverageDuration(), row);
            }
            public int? GetAverageDuration()
            {
                if (!_stopwatch.IsRunning)
                {
                    _stopwatch.Start();
                    return null;
                }
                return (int)(_stopwatch.ElapsedMilliseconds / _count);
            }
        }
        public Stream(INodeContext sourceNode, IPushObservable<T> observable)
        {
            this.SourceNode = sourceNode;
            var rowTracer = new RowTracer();
            var executionContext = this.SourceNode.ExecutionContext;

            this.Observable = observable.Filter(i => !executionContext.Terminating);

            if (!this.SourceNode.ExecutionContext.IsTracingContext)
            {
                if (executionContext.UseDetailedTraces)
                    PushObservable.Merge<ITraceContent>(
                        this.Observable.Map(rowTracer.ProcessRow),
                        this.Observable.Count().Map(count => new CounterSummaryStreamTraceContent(count)),
                        this.Observable.ExceptionsToObservable().Map(e => new UnhandledExceptionStreamTraceContent(e))
                    ).Do(i => this.SourceNode.ExecutionContext.AddTrace(i, sourceNode));
                else
                    PushObservable.Merge<ITraceContent>(
                        this.Observable.Count().Map(count => new CounterSummaryStreamTraceContent(count)),
                        this.Observable.ExceptionsToObservable().Map(e => new UnhandledExceptionStreamTraceContent(e))
                    ).Do(i => this.SourceNode.ExecutionContext.AddTrace(i, sourceNode));
            }
        }

        public IPushObservable<T> Observable { get; }

        public INodeContext SourceNode { get; }

        public virtual object GetMatchingStream<TOut>(INodeContext sourceNode, object observable)
        {
            return new Stream<TOut>(sourceNode, (IPushObservable<TOut>)observable);
        }
    }
}
