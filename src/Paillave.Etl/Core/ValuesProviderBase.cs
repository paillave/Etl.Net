using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Paillave.Etl.Core
{
    internal class SemaphoreOpen : IDisposable
    {
        private Semaphore _semaphore;
        public SemaphoreOpen(Semaphore semaphore)
        {
            _semaphore = semaphore;
            _semaphore.WaitOne();
        }
        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    _semaphore.Release();
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
    public abstract class ValuesProviderBase<TIn, TOut> : IValuesProvider<TIn, TOut>
    {
        private Semaphore _semaphore;
        protected IDisposable OpenProcess()
        {
            return new SemaphoreOpen(_semaphore);
        }
        public ValuesProviderBase(bool noParallelisation)
        {
            _semaphore = noParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
        }
        public virtual IDeferredPushObservable<TOut> PushValues(TIn input)
        {
            return new DeferredPushObservable<TOut>(pushValue => PushValues(input, pushValue));
        }

        protected virtual void PushValues(TIn input, Action<TOut> pushValue) { }
    }
    public abstract class ValuesProviderBase<TIn, TRes, TOut> : IValuesProvider<TIn, TRes, TOut>
    {
        private Semaphore _semaphore;
        protected IDisposable OpenProcess()
        {
            return new SemaphoreOpen(_semaphore);
        }
        public ValuesProviderBase(bool noParallelisation)
        {
            _semaphore = noParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
        }
        public virtual IDeferredPushObservable<TOut> PushValues(TRes resource, TIn input)
        {
            return new DeferredPushObservable<TOut>(pushValue => PushValues(resource, input, pushValue));
        }

        protected virtual void PushValues(TRes resource, TIn input, Action<TOut> pushValue) { }
    }
}
