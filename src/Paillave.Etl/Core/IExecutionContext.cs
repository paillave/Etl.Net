using Paillave.Etl.Reactive.Core;
using System;
using System.Threading.Tasks;

namespace Paillave.Etl.Core;

public interface IExecutionContext
{
    Guid ExecutionId { get; }
    bool UseDetailedTraces { get; }
    /// <summary>When set, every stream counts its rows in it. Null by default (no cost).</summary>
    LiveCounters? LiveCounters => null;
    bool Terminating { get; }
    void AddNode<T>(INodeDescription nodeContext, IPushObservable<T> observable);
    Task GetCompletionTask();
    void AddStreamToNodeLink(StreamToNodeLink link);

    // IMemoryCache ContextBag { get; }
    bool IsTracingContext { get; }
    void AddTrace(ITraceContent traceContent, INodeContext sourceNode);
    // IFileValueConnectors Connectors { get; }
    void AddDisposable(IDisposable disposable);
    IServiceProvider Services { get; }
}
