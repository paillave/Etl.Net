using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class CrossApplyArgs<TInMain, TInToApply, TValueIn, TValueOut, TOut>
    {
        public IStream<TInMain> MainStream { get; set; }
        public IStream<TInToApply> StreamToApply { get; set; }
        public Func<TInMain, TInToApply, TValueIn> GetValueIn { get; set; }
        public Func<TValueOut, TInMain, TInToApply, TOut> GetValueOut { get; set; }
        public IValuesProvider<TValueIn, TInToApply, TValueOut> ValuesProvider { get; set; }
    }
    public class CrossApplyArgs<TIn, TValueIn, TValueOut, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public Func<TIn, TValueIn> GetValueIn { get; set; }
        public Func<TValueOut, TIn, TOut> GetValueOut { get; set; }
        public IValuesProvider<TValueIn, TValueOut> ValuesProvider { get; set; }
    }
    public class CrossApplyStreamNode<TInMain, TInToApply, TValueIn, TValueOut, TOut> : StreamNodeBase<TOut, IStream<TOut>, CrossApplyArgs<TInMain, TInToApply, TValueIn, TValueOut, TOut>>
    {
        public CrossApplyStreamNode(string name, CrossApplyArgs<TInMain, TInToApply, TValueIn, TValueOut, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(CrossApplyArgs<TInMain, TInToApply, TValueIn, TValueOut, TOut> args)
        {
            var ob = args.MainStream.Observable.CombineWithLatest(args.StreamToApply.Observable, (m, a) => new { Main = m, Apply = a });
            return base.CreateUnsortedStream(ob.FlatMap(i =>
            {
                var def = args.ValuesProvider.PushValues(i.Apply, args.GetValueIn(i.Main, i.Apply));
                return new DeferedWrapperPushObservable<TOut>(def.Map(o => args.GetValueOut(o, i.Main, i.Apply)), def.Start);
            }));
        }
    }
    public class CrossApplyStreamNode<TInMain, TValueIn, TValueOut, TOut> : StreamNodeBase<TOut, IStream<TOut>, CrossApplyArgs<TInMain, TValueIn, TValueOut, TOut>>
    {
        public CrossApplyStreamNode(string name, CrossApplyArgs<TInMain, TValueIn, TValueOut, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(CrossApplyArgs<TInMain, TValueIn, TValueOut, TOut> args)
        {
            return base.CreateUnsortedStream(args.Stream.Observable.FlatMap(i =>
            {
                var def = args.ValuesProvider.PushValues(args.GetValueIn(i));
                return new DeferedWrapperPushObservable<TOut>(def.Map(o => args.GetValueOut(o, i)), def.Start);
            }));
        }
    }
}
