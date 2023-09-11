using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using System;
using System.Threading.Tasks;

namespace Paillave.Etl.Core
{

    public interface IExecutionContext
    {
        Guid ExecutionId { get; }
        bool UseDetailedTraces { get; }
        string JobName { get; }
        bool Terminating { get; }
        // int NextTraceSequence();
        void AddNode<T>(INodeDescription nodeContext, IPushObservable<T> observable);
        Task GetCompletionTask();
        void AddStreamToNodeLink(StreamToNodeLink link);

        SimpleDependencyResolver ContextBag { get; }
        bool IsTracingContext { get; }
        void AddTrace(ITraceContent traceContent, INodeContext sourceNode);
        IFileValueConnectors Connectors { get; }
        Task InvokeInDedicatedThreadAsync(object threadOwner, Func<Task> action);
        Task<T> InvokeInDedicatedThreadAsync<T>(object threadOwner, Func<Task<T>> action);
        Task InvokeInDedicatedThreadAsync(object threadOwner, Action action);
        Task<T> InvokeInDedicatedThreadAsync<T>(object threadOwner, Func<T> action);
        void AddDisposable(IDisposable disposable);
        IDependencyResolver DependencyResolver { get; }
    }
}
