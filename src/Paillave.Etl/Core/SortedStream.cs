using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.Core;

namespace Paillave.Etl.Core
{
    public class SortedStream<T, TKey> : Stream<T>, ISortedStream<T, TKey>
    {
        public SortedStream(INodeContext sourceNode, IPushObservable<T> observable, SortDefinition<T, TKey> sortDefinition)
            : base(sourceNode, observable)
        {
            if (sortDefinition == null) throw new ArgumentOutOfRangeException(nameof(sortDefinition), "sorting criteria list cannot be empty");
            this.SortDefinition = sortDefinition;
        }
        public SortDefinition<T, TKey> SortDefinition { get; }

        public override object GetMatchingStream<TOut>(INodeContext sourceNode, object observable) =>
            // TODO: the following is an absolute dreadful solution about which I MUST find a proper alternative
            new SortedStream<TOut, TKey>(sourceNode, (IPushObservable<TOut>)observable, (SortDefinition<TOut, TKey>)((object)this.SortDefinition));
    }
}
