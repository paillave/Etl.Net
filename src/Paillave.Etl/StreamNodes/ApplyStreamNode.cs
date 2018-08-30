using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class ApplyArgs<TInMain, TInToApply, TOut>
    {
        public IStream<TInMain> MainStream { get; set; }
        public IStream<TInToApply> StreamToApply { get; set; }
        public Func<TInMain, TInToApply, TOut> Selector { get; set; }
        public Func<TInMain, TInToApply, int, TOut> IndexSelector { get; set; }
        public bool ExcludeNull { get; set; }
    }
    public class ApplyStreamNode<TInMain, TInToApply, TOut> : StreamNodeBase<TOut, IStream<TOut>, ApplyArgs<TInMain, TInToApply, TOut>>
    {
        public ApplyStreamNode(string name, ApplyArgs<TInMain, TInToApply, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(ApplyArgs<TInMain, TInToApply, TOut> args)
        {
            IPushObservable<TOut> obs;
            if (args.IndexSelector == null)
                obs = args.MainStream.Observable.CombineWithLatest(args.StreamToApply.Observable.First(), WrapSelectForDisposal(args.Selector), true);
            else
                obs = args.MainStream.Observable.Map((e, i) => new IndexedObject<TInMain>(i, e)).CombineWithLatest(args.StreamToApply.Observable.First(), WrapSelectIndexObjectForDisposal(args.IndexSelector), true);
            if (args.ExcludeNull)
                obs = obs.Filter(i => i != null);
            return base.CreateUnsortedStream(obs);
        }
    }
}
