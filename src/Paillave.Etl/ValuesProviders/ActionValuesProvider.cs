using Paillave.Etl.Core;
using Paillave.Etl.Helpers;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Paillave.Etl.ValuesProviders
{
    public class ActionValuesProviderArgs<TIn, TOut>
    {
        public bool NoParallelisation { get; set; } = false;
        public Action<TIn, Action<TOut>> ProduceValues { get; set; }
    }
    public class ActionValuesProvider<TIn, TOut> : ValuesProviderBase<TIn, TOut>
    {
        private ActionValuesProviderArgs<TIn, TOut> _args;
        public ActionValuesProvider(ActionValuesProviderArgs<TIn, TOut> args) : base(args.NoParallelisation)
        {
            _args = args;
        }
        public override IDeferedPushObservable<TOut> PushValues(TIn input)
        {
            return new DeferedPushObservable<TOut>(pushValue =>
            {
                WaitOne();
                _args.ProduceValues(input, pushValue);
                Release();
            });
        }
    }
    public class ActionResourceValuesProviderArgs<TIn, TRes, TOut>
    {
        public bool NoParallelisation { get; set; } = false;
        public Action<TIn, TRes, Action<TOut>> ProduceValues { get; set; }
    }
    public class ActionResourceValuesProvider<TIn, TRes, TOut> : ValuesProviderBase<TIn, TRes, TOut>
    {
        private ActionResourceValuesProviderArgs<TIn, TRes, TOut> _args;
        public ActionResourceValuesProvider(ActionResourceValuesProviderArgs<TIn, TRes, TOut> args) : base(args.NoParallelisation)
        {
            _args = args;
        }
        public override IDeferedPushObservable<TOut> PushValues(TRes resource, TIn input)
        {
            return new DeferedPushObservable<TOut>(pushValue =>
            {
                WaitOne();
                _args.ProduceValues(input, resource, pushValue);
                Release();
            });
        }
    }
}
