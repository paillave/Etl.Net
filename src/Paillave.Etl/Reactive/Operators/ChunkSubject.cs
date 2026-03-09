using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators;

public class ChunkSubject<TIn> : PushSubject<IEnumerable<TIn>>
{
    private readonly IDisposable _subscription;
    private readonly object syncLock = new();
    private IList<TIn> _chunk = new List<TIn>();
    private readonly int _chunkSize;

    public ChunkSubject(IPushObservable<TIn> sourceS, int chunkSize) : base(sourceS.CancellationToken)
    {
        lock (syncLock)
        {
            this._chunkSize = chunkSize;
            this._subscription = sourceS.Subscribe(this.HandleOnPush, this.HandleOnComplete, this.HandleOnException);
        }
    }

    private void HandleOnException(Exception ex)
    {
        this.PushException(ex);
    }

    private void HandleOnComplete()
    {
        lock (syncLock)
        {
            if (_chunk.Count > 0)
                this.PushValue(_chunk);
            _chunk = new List<TIn>();
            this.Complete();
        }
    }

    private void HandleOnPush(TIn obj)
    {
        if (CancellationToken.IsCancellationRequested)
        {
            return;
        }
        lock (syncLock)
        {
            _chunk.Add(obj);
            if (_chunk.Count == _chunkSize && _chunkSize != 0)
            {
                this.PushValue(_chunk);
                _chunk = new List<TIn>();
            }
        }
    }

    public override void Dispose()
    {
        lock (syncLock)
        {
            base.Dispose();
            _subscription.Dispose();
        }
    }
}

public static partial class ObservableExtensions
{
    public static IPushObservable<IEnumerable<TIn>> Chunk<TIn>(this IPushObservable<TIn> sourceS, int chunkSize)
    {
        return new ChunkSubject<TIn>(sourceS, chunkSize);
    }
}
