using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Paillave.Etl.Core
{
    public abstract class ValuesProviderBase<TIn, TOut> : IValuesProvider<TIn, TOut>
    {
        private Synchronizer _synchronizer;
        protected IDisposable OpenProcess()
        {
            return _synchronizer.WaitBeforeProcess();
        }
        public ValuesProviderBase(bool noParallelisation)
        {
            _synchronizer = new Synchronizer(noParallelisation);
        }
        public virtual IDeferredPushObservable<TOut> PushValues(TIn input)
        {
            return new DeferredPushObservable<TOut>(pushValue =>
            {
                using (OpenProcess())
                    PushValues(input, pushValue);
            });
        }
        protected virtual void PushValues(TIn input, Action<TOut> pushValue) { }
    }
    public abstract class ValuesProviderBase<TIn, TResource, TOut> : IValuesProvider<TIn, TResource, TOut>
    {
        private Synchronizer _synchronizer;
        protected IDisposable OpenProcess()
        {
            return _synchronizer.WaitBeforeProcess();
        }
        public ValuesProviderBase(bool noParallelisation)
        {
            _synchronizer = new Synchronizer(noParallelisation);
        }
        public virtual IDeferredPushObservable<TOut> PushValues(TResource resource, TIn input)
        {
            return new DeferredPushObservable<TOut>(pushValue =>
            {
                using (OpenProcess())
                    PushValues(resource, input, pushValue);
            });
        }

        protected virtual void PushValues(TResource resource, TIn input, Action<TOut> pushValue) { }
    }
}
