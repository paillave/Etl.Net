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
        //protected WaitHandle WaitHandle { get; private set; }
        public virtual IDeferedPushObservable<TOut> PushValues(TIn args)
        {
            return new DeferedPushObservable<TOut>(pushValue => PushValues(args, pushValue));
        }

        protected virtual void PushValues(TIn args, Action<TOut> pushValue) { }

        //public void SetWaitHandle(WaitHandle waitHandle)
        //{
        //    WaitHandle = waitHandle;
        //}
    }
}
