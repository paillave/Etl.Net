using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core
{
    public class Disposable<T> : IDisposable
    {
        public T Resource { get; }
        private Action<T> _dispose = null;
        public Disposable(T resource, Action<T> dispose = null)
        {
            this.Resource = resource;
            this._dispose = dispose;
        }

        public void Dispose()
        {
            this._dispose(this.Resource);
        }
    }
}
