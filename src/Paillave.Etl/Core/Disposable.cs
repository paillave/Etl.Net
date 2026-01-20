using System;

namespace Paillave.Etl.Core;

public class Disposable<T>(T resource, Action<T> dispose = null) : IDisposable
{
    public T Resource { get; } = resource;
    private readonly Action<T> _dispose = dispose;

    public void Dispose()
    {
        this._dispose(this.Resource);
    }
}
