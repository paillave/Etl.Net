using System;
using System.Threading;

namespace Paillave.Etl.Core
{
    public interface IFileValueProcessor
    {
        string Code { get; }
        ProcessImpact PerformanceImpact { get; }
        ProcessImpact MemoryFootPrint { get; }
        void Process(IFileValue fileValue, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker);
        void Test();
    }
}