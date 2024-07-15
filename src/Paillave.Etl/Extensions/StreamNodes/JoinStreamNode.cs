using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;

namespace Paillave.Etl.Core
{
    public class JoinArgs<TInLeft, TInRight, TOut, TKey>
    {
        public ISortedStream<TInLeft, TKey> LeftInputStream { get; set; }
        public IKeyedStream<TInRight, TKey> RightInputStream { get; set; }
        public Func<TInLeft, TInRight, TOut> ResultSelector { get; set; }
        public bool RedirectErrorsInsteadOfFail { get; set; }
    }
    public class JoinStreamNode<TInLeft, TInRight, TOut, TKey>(string name, JoinArgs<TInLeft, TInRight, TOut, TKey> args) : StreamNodeBase<TOut, IStream<TOut>, JoinArgs<TInLeft, TInRight, TOut, TKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TOut> CreateOutputStream(JoinArgs<TInLeft, TInRight, TOut, TKey> args) =>
            base.CreateUnsortedStream(args.LeftInputStream.Observable.LeftJoin(args.RightInputStream.Observable, new SortDefinitionComparer<TInLeft, TInRight, TKey>(args.LeftInputStream.SortDefinition, args.RightInputStream.SortDefinition), args.ResultSelector));
    }
}
