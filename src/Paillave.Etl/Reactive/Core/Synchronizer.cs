using System;
using System.Threading;

namespace Paillave.Etl.Reactive.Core
{
    public class Synchronizer
    {
        private Semaphore _semaphore;
        public IDisposable WaitBeforeProcess()
        {
            return new SemaphoreAwaiter(_semaphore);
        }
        public Synchronizer(bool noParallelisation = false)
        {
            _semaphore = noParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
        }
        private class SemaphoreAwaiter : IDisposable
        {
            private Semaphore _semaphore;
            public SemaphoreAwaiter(Semaphore semaphore)
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
    }
}
