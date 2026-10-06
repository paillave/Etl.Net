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
    /// <param name="useNewVersion">when true, the pushed file value corresponds to the file that has just been saved on the connector (if the connector supports it) instead of the input file value</param>
    void Process(IFileValue fileValue, Action<IFileValue> push, bool useNewVersion, CancellationToken cancellationToken)
        => Process(fileValue, push, cancellationToken);
    IAsyncEnumerable<IFileValue> ProcessAsync(IFileValue fileValue, CancellationToken cancellationToken = default);
    void Test();
}