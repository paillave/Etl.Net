using System;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.Connector
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