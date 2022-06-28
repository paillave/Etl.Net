using Cronos;

namespace Paillave.Etl.Scheduler;
public class TickSource : TickSource<string>
{
    public TickSource(string cronExpression) : base(cronExpression, i => i) { }
    public static TickSource<TSource> Create<TSource>(TSource sourceMetadata, Func<TSource, string> getCronExpression) => new TickSource<TSource>(sourceMetadata, getCronExpression);
}
public class TickSource<TSource> : IDisposable
{
    private class TickRunningContext : IDisposable
    {
        private CancellationTokenSource _internalCancellationTokenSource;
        private CancellationTokenSource _combinedCancellationTokenSource;
        // private System.Timers.Timer? _timer = null;
        public TickRunningContext(CancellationToken externalCancellationToken, Action onStop)
        {
            _internalCancellationTokenSource = new CancellationTokenSource();
            _combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken, _internalCancellationTokenSource.Token);
        }
        public CancellationToken CancellationToken => _combinedCancellationTokenSource.Token;

        public void Dispose()
        {
            _internalCancellationTokenSource.Dispose();
            _combinedCancellationTokenSource.Dispose();
        }
        public void Stop()
        {
            _internalCancellationTokenSource.Cancel();
        }
    }
    private class TickSubscription : IDisposable
    {
        public TickSubscription(Action<TSource> push, Action<TickSubscription> unsubscribe) => (Push, _unsubscribe) = (push, unsubscribe);
        public void Dispose() => _unsubscribe(this);
        private readonly Action<TickSubscription> _unsubscribe;
        public Action<TSource> Push { get; }
    }
    public void UpdateSource(TSource? source = default)
    {
        source ??= this.Source;
        lock (this._syncObject)
        {
            this.Source = source;
            if (Running) Restart();
        }
    }
    private void Restart()
    {
        lock (_syncObject)
        {
            if (_timer != null && this._runningContext != null)
            {
                _timer.Stop();
                Task.Run(() => this.ScheduleNextTick(this._runningContext.CancellationToken, DateTime.Now), this._runningContext.CancellationToken);
            }
        }
    }
    private bool disposedValue;
    private readonly object _syncObject = new Object();
    private System.Timers.Timer? _timer = null;
    public TSource Source { get; private set; }
    private readonly List<TickSubscription> _subscriptions = new List<TickSubscription>();
    private Func<TSource, string> _getCronExpression;
    private TickRunningContext? _runningContext = null;
    public bool Running => _runningContext != null;
    public TickSource(TSource sourceMetadata, Func<TSource, string> getCronExpression)
        => (Source, _getCronExpression) = (sourceMetadata, getCronExpression);
    public IDisposable Subscribe(Action<TSource> push)
    {
        var tickSubscription = new TickSubscription(push, ts => _subscriptions.Remove(ts));
        _subscriptions.Add(tickSubscription);
        return tickSubscription;
    }
    public void Start() => Start(CancellationToken.None);

    public void Start(CancellationToken cancellationToken)
    {
        lock (_syncObject)
        {
            if (Running) throw new Exception("Running already");
            this._runningContext = new TickRunningContext(cancellationToken, this.Stop);
            Task.Run(() => this.ScheduleNextTick(this._runningContext.CancellationToken, DateTime.Now), this._runningContext.CancellationToken);
        }
    }
    private void ScheduleNextTick(CancellationToken cancellationToken, DateTime now)
    {
        lock (this._syncObject)
        {
            var next = CronExpression.Parse(this._getCronExpression(this.Source)).GetNextOccurrence(now.ToUniversalTime(), TimeZoneInfo.Local);
            // Console.WriteLine($"{now:hh mm ss fff} -> {next:hh mm ss fff}");
            if (next == null) return;
            var totalMilliseconds = (next.Value - DateTimeOffset.Now).TotalMilliseconds;
            totalMilliseconds = Math.Max(totalMilliseconds, 1);
            _timer = new System.Timers.Timer(totalMilliseconds);
            _timer.Elapsed += (sender, args) => PushTicks(cancellationToken, next.Value);
            _timer.Start();
        }
    }
    private void PushTicks(CancellationToken cancellationToken, DateTime now)
    {
        lock (this._syncObject)
        {
            if (_timer == null) return;
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
            foreach (var subscription in _subscriptions)
                Task.Run(() => subscription.Push(this.Source), cancellationToken);
            this.ScheduleNextTick(cancellationToken, now);
        }
    }
    public void Stop()
    {
        lock (_syncObject)
        {
            if (this._runningContext == null) throw new Exception("Not running yet");
            this._runningContext.Stop();
            this._runningContext.Dispose();
            this._runningContext = null;
            this._timer?.Stop();
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
            if (this._runningContext != null)
            {
                this._runningContext.Stop();
                this._runningContext.Dispose();
                this._runningContext = null;
            }
            if (this._timer != null)
            {
                this._timer.Stop();
                this._timer.Dispose();
            }
            disposedValue = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}