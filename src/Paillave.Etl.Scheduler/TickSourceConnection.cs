namespace Paillave.Etl.Scheduler;

public interface ITickSourceConnection<TSource, TKey> : IDisposable where TKey : IEquatable<TKey>
{
    public event EventHandler<ITickSourceChange<TSource, TKey>> Changed;
    public event EventHandler Stopped;
    IEnumerable<TSource> GetAll();
    TKey GetKey(TSource source);
    string? GetCronExpression(TSource source);
}
public interface ITickSourceChange<TSource, TKey> where TKey : IEquatable<TKey> { }
public class RemoveSourceChange<TSource, TKey> : ITickSourceChange<TSource, TKey> where TKey : IEquatable<TKey>
{
    public RemoveSourceChange(TKey key) => this.Key = key;
    public TKey Key { get; }
}
public class SaveSourceChange<TSource, TKey> : ITickSourceChange<TSource, TKey> where TKey : IEquatable<TKey>
{
    public SaveSourceChange(TSource source) => this.Source = source;
    public TSource Source { get; }
}

public abstract class TickSourceConnection<TSource, TKey> : ITickSourceConnection<TSource, TKey> where TKey : IEquatable<TKey>
{
    public event EventHandler<ITickSourceChange<TSource, TKey>>? Changed = null;
    protected virtual void OnChanged(ITickSourceChange<TSource, TKey> e)
        => this.Changed?.Invoke(this, e);
    public event EventHandler? Stopped = null;
    protected virtual void OnStopped()
        => this.Stopped?.Invoke(this, new EventArgs());

    public void Dispose()
        => this.OnStopped();
    public void AddOrChangeSource(TSource source)
        => OnChanged(new SaveSourceChange<TSource, TKey>(source));
    public abstract IEnumerable<TSource> GetAll();
    public abstract string? GetCronExpression(TSource source);
    public abstract TKey GetKey(TSource source);
}
