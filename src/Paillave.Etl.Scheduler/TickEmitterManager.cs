namespace Paillave.Etl.Scheduler;
public static class TickEmitterManager
{
    public static TickEmitterManager<TEmitter, TKey> Create<TEmitter, TKey>(Func<TEmitter, TKey> getKey, Action<TEmitter> push, Func<TEmitter, string> getCronExpression) where TKey : IEquatable<TKey>
        => new TickEmitterManager<TEmitter, TKey>(getKey, push, getCronExpression);
}
public class TickEmitterManager<TEmitter, TKey> : IDisposable where TKey : IEquatable<TKey>
{
    private class EmitterOccurrence : IDisposable
    {
        public EmitterOccurrence(TickEmitter<TEmitter> tickEmitter, IDisposable releaser) => (TickEmitter, Releaser) = (tickEmitter, releaser);
        public TickEmitter<TEmitter> TickEmitter { get; }
        public IDisposable Releaser { get; }

        public void Dispose()
        {
            this.Releaser.Dispose();
            this.TickEmitter.Dispose();
        }
    }
    private readonly object _lock = new object();
    private readonly Func<TEmitter, TKey> _getKey;
    private readonly Func<TEmitter, string> _getCronExpression;
    private readonly Action<TEmitter> _push;
    public TickEmitterManager(Func<TEmitter, TKey> getKey, Action<TEmitter> push, Func<TEmitter, string> getCronExpression) => (_getKey, _push, _getCronExpression) = (getKey, push, getCronExpression);
    private readonly Dictionary<TKey, EmitterOccurrence> _tickEmitters = new Dictionary<TKey, EmitterOccurrence>();
    private bool disposedValue;

    public void SetEmitter(TEmitter emitter)
    {
        lock (_lock)
        {
            if (_tickEmitters.TryGetValue(_getKey(emitter), out var occurrence)) occurrence.TickEmitter.UpdateEmitter(emitter);
            else AddEmitter(emitter);
        }
    }
    private void AddEmitter(TEmitter emitter)
    {
        lock (_lock)
        {
            var tickEmitter = new TickEmitter<TEmitter>(emitter, this._getCronExpression);
            var releaser = tickEmitter.Subscribe(_push);
            var occurrence = new EmitterOccurrence(tickEmitter, releaser);
            _tickEmitters[_getKey(emitter)] = occurrence;
            tickEmitter.Start();
        }
    }
    public void RemoveEmitter(TKey emitterKey)
    {
        lock (_lock)
        {
            if (!_tickEmitters.TryGetValue(emitterKey, out var occurrence)) return;
            if (occurrence == null) return;
            RemoveEmitter(occurrence);
        }
    }
    private void RemoveEmitter(EmitterOccurrence occurrence)
    {
        lock (_lock)
        {
            occurrence.Dispose();
            _tickEmitters.Remove(_getKey(occurrence.TickEmitter.Emitter));
        }
    }
    public void SynchronizeEmitters(IEnumerable<TEmitter> newEmitters)
    {
        lock (_lock)
        {
            var updates = _tickEmitters.Join(newEmitters, i => i.Key, _getKey, (l, r) => new { Previous = l.Value.TickEmitter, New = r });
            foreach (var tickEmitter in updates)
                tickEmitter.Previous.UpdateEmitter(tickEmitter.New);
            var toAdd = newEmitters.Where(n => !_tickEmitters.ContainsKey(_getKey(n))).ToList();
            foreach (var item in toAdd)
                AddEmitter(item);
            var toRemove = _tickEmitters.Values.Where(i => !newEmitters.Any(n => _getKey(n).Equals(_getKey(i.TickEmitter.Emitter)))).ToList();
            foreach (var item in toRemove)
                RemoveEmitter(item);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }
            lock (_lock)
                foreach (var item in _tickEmitters)
                    item.Value.Dispose();

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TickEmitterManager()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
