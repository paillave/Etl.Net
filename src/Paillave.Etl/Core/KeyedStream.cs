using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.Core;

namespace Paillave.Etl.Core;

public class KeyedStream<T, TKey>(INodeContext sourceNode, IPushObservable<T> observable, SortDefinition<T, TKey> sortDefinition, bool trace = true) : Stream<T>(sourceNode, observable, trace), IKeyedStream<T, TKey>
{
    public SortDefinition<T, TKey> SortDefinition { get; } = sortDefinition ?? throw new ArgumentOutOfRangeException(nameof(sortDefinition), "key criteria list cannot be empty");
    public override object GetMatchingStream<TOut>(INodeContext sourceNode, object observable)
    {
        // TODO: the following is an absolute dreadful solution about which I MUST find a proper alternative
        return new KeyedStream<TOut, TKey>(sourceNode, (IPushObservable<TOut>)observable, (SortDefinition<TOut, TKey>)((object)this.SortDefinition));
    }
}
