using System;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.Connector
{
    public interface IFileValueProvider
    {
        string Code { get; }
        ProcessImpact PerformanceImpact { get; }
        ProcessImpact MemoryFootPrint { get; }
        void Provide(Action<IFileValue> pushFileValue, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker);
        void Test();
    }
}