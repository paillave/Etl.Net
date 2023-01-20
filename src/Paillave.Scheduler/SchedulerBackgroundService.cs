using Microsoft.Extensions.Hosting;

namespace Paillave.Scheduler;
public abstract class SchedulerBackgroundService<TSource, TKey> : BackgroundService where TKey : IEquatable<TKey>
{
    private readonly ITickSourceConnection<TSource, TKey> _tickSourceConnection;
    private readonly IServiceProvider _serviceProvider;

    public SchedulerBackgroundService(IServiceProvider serviceProvider, ITickSourceConnection<TSource, TKey> tickSourceConnection)
        => (_tickSourceConnection, _serviceProvider) = (tickSourceConnection, serviceProvider);
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(() =>
    {
        using (EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset))
        using (var tickSourceManager = TickSourceManager.Create(this._tickSourceConnection))
        {
            tickSourceManager.Tick += (sender, e) => Execute(e);
            _tickSourceConnection.Stopped += (sender, e) => waitHandle.Set();
            waitHandle.WaitOne();
        }
    }, stoppingToken);
    protected abstract void Execute(TSource source);
}
