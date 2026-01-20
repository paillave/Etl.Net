using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Core;

public class SingleStream<T>(INodeContext sourceNode, IPushObservable<T> observable, bool trace = true) : KeyedStream<T, T>(sourceNode, observable, new SortDefinition<T, T>(i => i), trace), ISingleStream<T>
{
    public override object GetMatchingStream<TOut>(INodeContext sourceNode, object observable)
    {
        // TODO: the following is an absolute dreadful solution about which I MUST find a proper alternative
        return new SingleStream<TOut>(sourceNode, (IPushObservable<TOut>)observable);
    }
}