using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.Core.TraceContents;

namespace Paillave.Etl.Core.Streams
{
    public class KeyedStream<T, TKey> : Stream<T>, IKeyedStream<T, TKey>
    {
        public KeyedStream(INodeContext sourceNode, IPushObservable<T> observable, SortDefinition<T, TKey> sortDefinition)
        : base(sourceNode, observable)
        {
            this.SortDefinition = sortDefinition ?? throw new ArgumentOutOfRangeException(nameof(sortDefinition), "key criteria list cannot be empty");
        }
        public SortDefinition<T, TKey> SortDefinition { get; }
        public override object GetMatchingStream<TOut>(INodeContext sourceNode, object observable)
        {
            // TODO: the following is an absolute dreadful solution about which I MUST find a proper alternative
            return new KeyedStream<TOut, TKey>(sourceNode, (IPushObservable<TOut>)observable, (SortDefinition<TOut, TKey>)((object)this.SortDefinition));
        }
    }
}
