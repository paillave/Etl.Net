using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class EnsureSortedArgs<T, TKey>
    {
        public IStream<T> Input { get; set; }
        public SortDefinition<T, TKey> SortDefinition { get; set; }
    }
    public class EnsureSortedStreamNode<TOut, TKey> : StreamNodeBase<TOut, ISortedStream<TOut, TKey>, EnsureSortedArgs<TOut, TKey>>
    {
        public EnsureSortedStreamNode(string name, EnsureSortedArgs<TOut, TKey> args) : base(name, args)
        {
        }

        protected override ISortedStream<TOut, TKey> CreateOutputStream(EnsureSortedArgs<TOut, TKey> args)
        {
            return base.CreateSortedStream(args.Input.Observable.ExceptionOnUnsorted(args.SortDefinition, true), args.SortDefinition);
        }
    }
}
