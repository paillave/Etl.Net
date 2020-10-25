using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class JoinArgs<TInLeft, TInRight, TOut, TKey>
    {
        public ISortedStream<TInLeft, TKey> LeftInputStream { get; set; }
        public IKeyedStream<TInRight, TKey> RightInputStream { get; set; }
        public Func<TInLeft, TInRight, TOut> ResultSelector { get; set; }
        public bool RedirectErrorsInsteadOfFail { get; set; }
    }
    public class JoinStreamNode<TInLeft, TInRight, TOut, TKey> : StreamNodeBase<TOut, IStream<TOut>, JoinArgs<TInLeft, TInRight, TOut, TKey>>
    {
        public JoinStreamNode(string name, JoinArgs<TInLeft, TInRight, TOut, TKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TOut> CreateOutputStream(JoinArgs<TInLeft, TInRight, TOut, TKey> args)
        {
            return base.CreateUnsortedStream(args.LeftInputStream.Observable.LeftJoin(args.RightInputStream.Observable, new SortDefinitionComparer<TInLeft, TInRight, TKey>(args.LeftInputStream.SortDefinition, args.RightInputStream.SortDefinition), args.ResultSelector));
        }
    }
}
