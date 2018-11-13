using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class CrossApplyArgs<TInMain, TInToApply, TValueIn, TValueOut, TOut>
    {
        public bool NoParallelisation { get; set; } = true;
        public IStream<TInMain> MainStream { get; set; }
        public ISingleStream<TInToApply> StreamToApply { get; set; }
        public Func<TInMain, TInToApply, TValueIn> GetValueIn { get; set; }
        public Func<TValueOut, TInMain, TInToApply, TOut> GetValueOut { get; set; }
        public Action<TValueIn, TInToApply, Action<TValueOut>> ValuesProvider { get; set; }
    }
    public class CrossApplyArgs<TIn, TValueIn, TValueOut, TOut>
    {
        public bool NoParallelisation { get; set; } = true;
        public IStream<TIn> Stream { get; set; }
        public Func<TIn, TValueIn> GetValueIn { get; set; }
        public Func<TValueOut, TIn, TOut> GetValueOut { get; set; }
        public Action<TValueIn, Action<TValueOut>> ValuesProvider { get; set; }
    }
    public class CrossApplyStreamNode<TInMain, TInToApply, TValueIn, TValueOut, TOut> : StreamNodeBase<TOut, IStream<TOut>, CrossApplyArgs<TInMain, TInToApply, TValueIn, TValueOut, TOut>>
    {
        public CrossApplyStreamNode(string name, CrossApplyArgs<TInMain, TInToApply, TValueIn, TValueOut, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(CrossApplyArgs<TInMain, TInToApply, TValueIn, TValueOut, TOut> args)
        {
            var ob = args.MainStream.Observable.CombineWithLatest(args.StreamToApply.Observable.First(), (m, a) => new Tuple<TInMain, TInToApply>(m, a));
            if (args.NoParallelisation)
            {
                return base.CreateUnsortedStream(ob.MultiMap<Tuple<TInMain, TInToApply>, TOut>((Tuple<TInMain, TInToApply> i, Action<TOut> push) =>
                {
                    var inputValue = args.GetValueIn(i.Item1, i.Item2);
                    Action<TValueOut> newPush = (TValueOut e) => push(args.GetValueOut(e, i.Item1, i.Item2));
                    args.ValuesProvider(inputValue, i.Item2, newPush);
                }));
            }
            else
            {
                var synchronizer = new Synchronizer();
                return base.CreateUnsortedStream(ob.FlatMap(i => new DeferredPushObservable<TOut>(push =>
                    {
                        var inputValue = args.GetValueIn(i.Item1, i.Item2);
                        Action<TValueOut> newPush = (TValueOut e) => push(args.GetValueOut(e, i.Item1, i.Item2));
                        using (synchronizer.WaitBeforeProcess())
                            args.ValuesProvider(inputValue, i.Item2, newPush);
                    })
                ));
            }
        }
    }
    public class CrossApplyStreamNode<TInMain, TValueIn, TValueOut, TOut> : StreamNodeBase<TOut, IStream<TOut>, CrossApplyArgs<TInMain, TValueIn, TValueOut, TOut>>
    {
        public CrossApplyStreamNode(string name, CrossApplyArgs<TInMain, TValueIn, TValueOut, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(CrossApplyArgs<TInMain, TValueIn, TValueOut, TOut> args)
        {
            if (args.NoParallelisation)
            {
                return base.CreateUnsortedStream(args.Stream.Observable.MultiMap<TInMain, TOut>((TInMain i, Action<TOut> push) =>
                {
                    var inputValue = args.GetValueIn(i);
                    Action<TValueOut> newPush = (TValueOut e) => push(args.GetValueOut(e, i));
                    args.ValuesProvider(inputValue, newPush);
                }));
            }
            else
            {
                var synchronizer = new Synchronizer();
                return base.CreateUnsortedStream(args.Stream.Observable.FlatMap(i =>
                {
                    return new DeferredPushObservable<TOut>(push =>
                    {
                        var inputValue = args.GetValueIn(i);
                        Action<TValueOut> newPush = (TValueOut e) => push(args.GetValueOut(e, i));
                        using (synchronizer.WaitBeforeProcess())
                            args.ValuesProvider(inputValue, newPush);
                    });
                }
                ));
            }
        }
    }
}
