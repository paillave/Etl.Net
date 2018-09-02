using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Disposables
{
    public class SingleDisposableManager : IDisposableManager
    {
        private IDisposable _disposable = null;

        private object syncLock = new object();
        public void Dispose()
        {
            lock (syncLock)
            {
                _disposable?.Dispose();
                _disposable = null;
            }
        }
        public void Set(IDisposable disposable)
        {
            lock (syncLock)
            {
                _disposable?.Dispose();
                _disposable = disposable;
            }
        }
        public void TryDispose(IDisposable disposable)
        {
            lock (syncLock)
            {
                if (Object.ReferenceEquals(_disposable, disposable))
                {
                    Dispose();
                }
            }
        }
        public bool IsDisposed => _disposable == null;
    }
}
