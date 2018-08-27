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
    public class EnsureKeyedArgs<T>
    {
        public IStream<T> Input { get; set; }
        public IEnumerable<SortCriteria<T>> Criterias { get; set; }
    }
    public class EnsureKeyedStreamNode<TOut> : StreamNodeBase<TOut, IKeyedStream<TOut>, EnsureKeyedArgs<TOut>>
    {
        public EnsureKeyedStreamNode(string name, EnsureKeyedArgs<TOut> args) : base(name, args)
        {
        }

        protected override IKeyedStream<TOut> CreateOutputStream(EnsureKeyedArgs<TOut> args)
        {
            return base.CreateKeyedStream(args.Input.Observable.ExceptionOnUnsorted(new SortCriteriaComparer<TOut>(args.Criterias.ToArray()), true), args.Criterias);
        }
    }
}
