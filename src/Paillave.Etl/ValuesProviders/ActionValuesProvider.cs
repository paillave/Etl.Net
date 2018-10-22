using Paillave.Etl.Core;
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
        public override IDeferredPushObservable<TOut> PushValues(TIn input)
        {
            return new DeferredPushObservable<TOut>(pushValue =>
            {
                using (base.OpenProcess())
                    _args.ProduceValues(input, pushValue);
            });
        }
    }
    public class ActionResourceValuesProviderArgs<TIn, TResource, TOut>
    {
        public bool NoParallelisation { get; set; } = false;
        public Action<TIn, TResource, Action<TOut>> ProduceValues { get; set; }
    }
    public class ActionResourceValuesProvider<TIn, TResource, TOut> : ValuesProviderBase<TIn, TResource, TOut>
    {
        private ActionResourceValuesProviderArgs<TIn, TResource, TOut> _args;
        public ActionResourceValuesProvider(ActionResourceValuesProviderArgs<TIn, TResource, TOut> args) : base(args.NoParallelisation)
        {
            _args = args;
        }
        public override IDeferredPushObservable<TOut> PushValues(TResource resource, TIn input)
        {
            return new DeferredPushObservable<TOut>(pushValue =>
            {
                using (base.OpenProcess())
                    _args.ProduceValues(input, resource, pushValue);
            });
        }
    }
}
