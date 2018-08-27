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
    public class SortArgs<T>
    {
        public IStream<T> Input { get; set; }
        public IEnumerable<SortCriteria<T>> Criterias { get; set; }
    }
    public class SortStreamNode<TOut> : StreamNodeBase<TOut, ISortedStream<TOut>, SortArgs<TOut>>
    {
        public SortStreamNode(string name, SortArgs<TOut> args) : base(name, args)
        {
        }

        protected override ISortedStream<TOut> CreateOutputStream(SortArgs<TOut> args)
        {
            return base.CreateSortedStream(args.Input.Observable.ToList().FlatMap(i =>
            {
                i.Sort(new SortCriteriaComparer<TOut>(args.Criterias.ToArray()));
                return PushObservable.FromEnumerable(i);
            }), args.Criterias);
        }
    }
}
