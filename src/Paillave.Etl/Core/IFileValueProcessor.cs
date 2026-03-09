using System;
using System.Collections.Generic;
using System.Threading;

namespace Paillave.Etl.Core;

public interface IFileValueProcessor
{
    string Code { get; }
    ProcessImpact PerformanceImpact { get; }
    ProcessImpact MemoryFootPrint { get; }
    void Process(IFileValue fileValue, Action<IFileValue> push, CancellationToken cancellationToken);
    IAsyncEnumerable<IFileValue> ProcessAsync(IFileValue fileValue, CancellationToken cancellationToken = default);
    void Test();
}