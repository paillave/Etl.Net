using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Disposables
{
    public class CollectionDisposableManager : IDisposableManager
    {
        private List<IDisposable> _disposables = new List<IDisposable>();

        private object syncLock = new object();
        public void Dispose()
        {
            lock (syncLock)
            {
                foreach (var item in _disposables)
                    try
                    {
                        item.Dispose();
                    }
                    catch { }
                _disposables.Clear();
            }
        }
        public T Set<T>(T disposable) where T : IDisposable
        {
            lock (syncLock)
            {
                if (disposable != null)
                    _disposables.Add(disposable);
                return disposable;
            }
        }
        public void AddRange(params IDisposable[] disposables)
        {
            lock (syncLock)
            {
                if (disposables != null)
                    foreach (var disposable in disposables)
                        _disposables.Add(disposable);
            }
        }
        public void TryDispose(IDisposable disposable)
        {
            lock (syncLock)
            {
                if (disposable == null) return;
                if (_disposables.Contains(disposable))
                {
                    _disposables.Remove(disposable);
                    disposable.Dispose();
                }
            }
        }
        public bool IsDisposed => _disposables.Count == 0;
    }
}
