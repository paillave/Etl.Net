using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class DistinctArgs<TIn, TKey>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
    }
    public class DistinctStreamNode<TIn, TKey> : StreamNodeBase<TIn, IStream<TIn>, DistinctArgs<TIn, TKey>>
    {
        public DistinctStreamNode(string name, DistinctArgs<TIn, TKey> args) : base(name, args)
        {
        }

        protected override IStream<TIn> CreateOutputStream(DistinctArgs<TIn, TKey> args)
        {
            return base.CreateUnsortedStream(args.InputStream.Observable.Distinct(new SortDefinition<TIn, TKey>(args.GetKey)));
        }
    }
}
