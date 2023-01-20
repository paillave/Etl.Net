namespace Paillave.Scheduler;
public static class TickSourceManager
{
    public static TickSourceManager<TSource, TKey> Create<TSource, TKey>(ITickSourceConnection<TSource, TKey> tickSourceConnection) where TKey : IEquatable<TKey>
        => new TickSourceManager<TSource, TKey>(tickSourceConnection);
}

public class TickSourceManager<TSource, TKey> : IDisposable where TKey : IEquatable<TKey>
{
    private class SourceOccurrence : IDisposable
    {
        public SourceOccurrence(TickSource<TSource> tickSource, IDisposable releaser) => (TickSource, Releaser) = (tickSource, releaser);
        public TickSource<TSource> TickSource { get; }
        public IDisposable Releaser { get; }

        public void Dispose()
        {
            this.Releaser.Dispose();
            this.TickSource.Dispose();
        }
    }
    public void Start()
    {
        foreach (var item in _tickSources)
            item.Value.TickSource.Start();
    }
    public void Stop()
    {
        foreach (var item in _tickSources)
            item.Value.TickSource.Stop();
    }
    private readonly object _lock = new object();
    public readonly ITickSourceConnection<TSource, TKey> _sourceConnection;
    public event EventHandler<TSource>? Tick = null;
    protected virtual void OnPushTick(TSource source)
        => this.Tick?.Invoke(this, source);
    public TickSourceManager(ITickSourceConnection<TSource, TKey> sourceConnection)
    {
        _sourceConnection = sourceConnection;
        sourceConnection.Changed += this.HandleSourceChange;
        this.ResetSources(this._sourceConnection.GetAll());
    }
    private void HandleSourceChange(object? sender, ITickSourceChange<TSource, TKey> change)
    {
        lock (_lock)
        {
            switch (change)
            {
                case RemoveSourceChange<TSource, TKey> removeSourceChange:
                    RemoveSource(removeSourceChange.Key);
                    break;
                case SaveSourceChange<TSource, TKey> saveSourceChange:
                    ResetSource(saveSourceChange.Source);
                    break;
            }
        }
    }
    private readonly Dictionary<TKey, SourceOccurrence> _tickSources = new Dictionary<TKey, SourceOccurrence>();
    private bool disposedValue;

    public void ResetSource(TSource source)
    {
        lock (_lock)
        {
            if (_tickSources.TryGetValue(_sourceConnection.GetKey(source), out var occurrence)) occurrence.TickSource.UpdateSource(source);
            else AddSource(source);
        }
    }
    private void AddSource(TSource source)
    {
        lock (_lock)
        {
            var tickSource = new TickSource<TSource>(source, _sourceConnection.GetCronExpression);
            var releaser = tickSource.Subscribe(OnPushTick);
            var occurrence = new SourceOccurrence(tickSource, releaser);
            _tickSources[_sourceConnection.GetKey(source)] = occurrence;
            // tickSource.Start();
        }
    }
    private void RemoveSource(TKey sourceKey)
    {
        lock (_lock)
        {
            if (!_tickSources.TryGetValue(sourceKey, out var occurrence)) return;
            if (occurrence == null) return;
            RemoveSource(occurrence);
        }
    }
    private void RemoveSource(SourceOccurrence occurrence)
    {
        lock (_lock)
        {
            occurrence.Dispose();
            _tickSources.Remove(_sourceConnection.GetKey(occurrence.TickSource.Source));
        }
    }
    private void ResetSources(IEnumerable<TSource> newSources)
    {
        lock (_lock)
        {
            var updates = _tickSources.Join(newSources, i => i.Key, _sourceConnection.GetKey, (l, r) => new { Previous = l.Value.TickSource, New = r });
            foreach (var tickSource in updates)
                tickSource.Previous.UpdateSource(tickSource.New);
            var toAdd = newSources.Where(n => !_tickSources.ContainsKey(_sourceConnection.GetKey(n))).ToList();
            foreach (var item in toAdd)
                AddSource(item);
            var toRemove = _tickSources.Values.Where(i => !newSources.Any(n => _sourceConnection.GetKey(n).Equals(_sourceConnection.GetKey(i.TickSource.Source)))).ToList();
            foreach (var item in toRemove)
                RemoveSource(item);
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
            {
                foreach (var item in _tickSources)
                    item.Value.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
