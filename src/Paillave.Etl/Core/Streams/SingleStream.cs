using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Core.Streams
{
    public class SingleStream<T> : KeyedStream<T, T>, ISingleStream<T>
    {
        public SingleStream(INodeContext sourceNode, IPushObservable<T> observable) : base(sourceNode, observable, new SortDefinition<T, T>(i => i))
        {
        }
        public override object GetMatchingStream<TOut>(INodeContext sourceNode, object observable)
        {
            // TODO: the following is an absolute dreadful solution about which I MUST find a proper alternative
            return new SingleStream<TOut>(sourceNode, (IPushObservable<TOut>)observable);
        }
    }
}