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
    public class SortArgs<T, TKey>
    {
        public IStream<T> Input { get; set; }
        public SortDefinition<T, TKey> SortDefinition { get; set; }
    }
    public class SortStreamNode<TOut, TKey> : StreamNodeBase<TOut, ISortedStream<TOut, TKey>, SortArgs<TOut, TKey>>
    {
        public SortStreamNode(string name, SortArgs<TOut, TKey> args) : base(name, args)
        {
        }

        protected override ISortedStream<TOut, TKey> CreateOutputStream(SortArgs<TOut, TKey> args)
        {
            return base.CreateSortedStream(args.Input.Observable.ToList().FlatMap(i =>
            {
                i.Sort(args.SortDefinition);
                return PushObservable.FromEnumerable(i);
            }), args.SortDefinition);
        }
    }
}
