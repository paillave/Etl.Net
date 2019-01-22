using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Core.Streams
{
    public class SingleStream<T> : KeyedStream<T, T>, ISingleStream<T>
    {
        public SingleStream(ITraceMapper tracer, IExecutionContext executionContext, string sourceNodeName, IPushObservable<T> observable) : base(tracer, executionContext, sourceNodeName, observable, new SortDefinition<T, T>(i => i))
        {
        }
        public override object GetMatchingStream(ITraceMapper tracer, IExecutionContext executionContext, string name, object observable)
        {
            return new SingleStream<T>(tracer, executionContext, name, (IPushObservable<T>)observable);
        }
    }
}