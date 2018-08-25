using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public class ApplyArgs<TInMain, TInToApply, TOut>
    {
        public IStream<TInMain> MainStream { get; set; }
        public IStream<TInToApply> StreamToApply { get; set; }
        public Func<TInMain, TInToApply, TOut> Selector { get; set; }
        public Func<TInMain, TInToApply, int, TOut> IndexSelector { get; set; }
    }
    public class ApplyStreamNode<TInMain, TInToApply, TOut> : StreamNodeBase<TOut, IStream<TOut>, ApplyArgs<TInMain, TInToApply, TOut>>
    {
        public ApplyStreamNode(string name, ApplyArgs<TInMain, TInToApply, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(ApplyArgs<TInMain, TInToApply, TOut> args)
        {
            if (args.IndexSelector == null)
                return base.CreateStream(args.MainStream.Observable.CombineWithLatest(args.StreamToApply.Observable.First(), WrapSelectForDisposal(args.Selector), true));
            else
                //TODO: optimize this
                return base.CreateStream(args.MainStream.Observable.Map((e, i) => new { Element = e, Index = i }).CombineWithLatest(args.StreamToApply.Observable.First(), (m, a) => WrapSelectIndexForDisposal<TInMain, TInToApply, TOut>((p1, p2, idx) => args.IndexSelector(p1, p2, idx))(m.Element, a, m.Index), true));
        }
    }
}
