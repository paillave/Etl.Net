using System;
using System.Threading;

namespace Paillave.Etl.Core
{
    public interface IFileValueProvider
    {
        string Code { get; }
        ProcessImpact PerformanceImpact { get; }
        ProcessImpact MemoryFootPrint { get; }
        void Provide(Action<IFileValue> pushFileValue, CancellationToken cancellationToken, IExecutionContext context);
        void Test();
    }
}