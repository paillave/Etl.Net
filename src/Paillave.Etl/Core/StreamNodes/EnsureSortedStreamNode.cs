using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public class EnsureSortedArgs<T>
    {
        public IStream<T> Input { get; set; }
        public IEnumerable<SortCriteria<T>> Criterias { get; set; }
    }
    public class EnsureSortedStreamNode<TOut> : StreamNodeBase<TOut, ISortedStream<TOut>, EnsureSortedArgs<TOut>>
    {
        public EnsureSortedStreamNode(string name, EnsureSortedArgs<TOut> args) : base(name, args)
        {
        }

        protected override ISortedStream<TOut> CreateOutputStream(EnsureSortedArgs<TOut> args)
        {
            return base.CreateSortedStream(args.Input.Observable.ExceptionOnUnsorted(new SortCriteriaComparer<TOut>(args.Criterias.ToArray())), args.Criterias);
        }
    }
}
