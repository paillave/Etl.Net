using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Paillave.Etl.Core
{
    public abstract class ValuesProviderBase<TIn, TOut> : IValuesProvider<TIn, TOut>
    {
        private Semaphore _semaphore;
        protected void WaitOne()
        {
            this._semaphore.WaitOne();
        }
        protected void Release()
        {
            this._semaphore.Release();
        }
        public ValuesProviderBase(bool noParallelisation)
        {
            _semaphore = noParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
        }
        public virtual IDeferedPushObservable<TOut> PushValues(TIn args)
        {
            return new DeferedPushObservable<TOut>(pushValue => PushValues(args, pushValue));
        }

        protected virtual void PushValues(TIn args, Action<TOut> pushValue) { }
    }
    public abstract class ValuesProviderBase<TIn, TRes, TOut> : IValuesProvider<TIn, TRes, TOut>
    {
        private Semaphore _semaphore;
        protected void WaitOne()
        {
            this._semaphore.WaitOne();
        }
        protected void Release()
        {
            this._semaphore.Release();
        }
        public ValuesProviderBase(bool noParallelisation)
        {
            _semaphore = noParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
        }
        public virtual IDeferedPushObservable<TOut> PushValues(TRes resource, TIn args)
        {
            return new DeferedPushObservable<TOut>(pushValue => PushValues(resource, args, pushValue));
        }

        protected virtual void PushValues(TRes resource, TIn args, Action<TOut> pushValue) { }
    }
}
