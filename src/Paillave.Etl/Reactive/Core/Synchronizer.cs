using System;
using System.Threading;

namespace Paillave.Etl.Reactive.Core;

public class Synchronizer(bool noParallelisation = false)
{
    private readonly SemaphoreSlim _semaphore = noParallelisation ? new SemaphoreSlim(1, 1) : new SemaphoreSlim(10, 10);
    public IDisposable WaitBeforeProcess()
    {
        return new SemaphoreAwaiter(_semaphore);
    }

    private class SemaphoreAwaiter : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        public SemaphoreAwaiter(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
            _semaphore.Wait();
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
