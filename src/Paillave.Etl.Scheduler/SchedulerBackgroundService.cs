using Microsoft.Extensions.Hosting;
using Paillave.Etl.Core;

// https://medium.com/@daniel.sagita/backgroundservice-for-a-long-running-work-3debe8f8d25b
// https://docs.microsoft.com/en-us/dotnet/core/extensions/queue-service
// https://docs.microsoft.com/en-us/dotnet/core/extensions/scoped-service
namespace Paillave.Etl.Scheduler;
public class SchedulerBackgroundService<TSource, TKey> : BackgroundService where TKey : IEquatable<TKey>
{
    private readonly ISchedulerBackgroundServiceOptions<TSource, TKey> _schedulerBackgroundServiceOptions;
    public SchedulerBackgroundService(ISchedulerBackgroundServiceOptions<TSource, TKey> schedulerBackgroundServiceOptions)
        => (_schedulerBackgroundServiceOptions) = (schedulerBackgroundServiceOptions);
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var executionOptions = new ExecutionOptions<object>
        {
            Resolver = _schedulerBackgroundServiceOptions.Resolver,
            CancellationToken = stoppingToken,
            Connectors = _schedulerBackgroundServiceOptions.Connectors,
            NoExceptionOnError = true,
            TraceProcessDefinition = (a, b) => _schedulerBackgroundServiceOptions.TraceProcessDefinition(a),
            TraceResolver = _schedulerBackgroundServiceOptions.TraceResolver,
            UseDetailedTraces = _schedulerBackgroundServiceOptions.UseDetailedTraces
        };

        var processRunner = StreamProcessRunner.Create<object>(i => i
            .EmitEvents("Emit events", _schedulerBackgroundServiceOptions.TickSourceConnection)
            .SubProcess("Execute Sub Process", i => _schedulerBackgroundServiceOptions.CreateProcess(i)));
        await processRunner.ExecuteAsync(null, executionOptions);
    }
}
public interface ISchedulerBackgroundServiceOptions<TSource, TKey> where TKey : IEquatable<TKey>
{
    bool UseDetailedTraces { get; set; }
    Action<IStream<TraceEvent>> TraceProcessDefinition { get; set; }
    IDependencyResolver Resolver { get; set; }
    IDependencyResolver TraceResolver { get; set; }
    IFileValueConnectors Connectors { get; set; }
    IStream<object> CreateProcess(ISingleStream<TSource> sourceStream);
    ITickSourceConnection<TSource, TKey> TickSourceConnection { get; set; }
}