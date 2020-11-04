using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.Connector;
using Paillave.Etl.Reactive.Core;
using System;
using System.Threading.Tasks;

namespace Paillave.Etl.Core
{

    public interface IExecutionContext : IInvoker
    {
        Guid ExecutionId { get; }
        string JobName { get; }
        // int NextTraceSequence();
        void AddNode<T>(INodeDescription nodeContext, IPushObservable<T> observable);
        void AddDisposable(IDisposable disposable);
        Task GetCompletionTask();
        void AddStreamToNodeLink(StreamToNodeLink link);

        IDependencyResolver DependencyResolver { get; }
        SimpleDependencyResolver ContextBag { get; }
        bool IsTracingContext { get; }
        void AddTrace(ITraceContent traceContent, INodeContext sourceNode);
        IFileValueConnectors Connectors { get; }
    }
    public interface IInvoker
    {
        void InvokeInDedicatedThread(object threadOwner, Action action);
        T InvokeInDedicatedThread<T>(object threadOwner, Func<T> action);
    }
}
