using Paillave.Etl.Reactive.Core;
using System;
using System.Threading.Tasks;

namespace Paillave.Etl.Core;

public interface IExecutionContext
{
    Guid ExecutionId { get; }
    bool UseDetailedTraces { get; }
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
