using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading;

namespace Paillave.Etl.Core;

public interface IFileValueProvider
{
    string Code { get; }
    ProcessImpact PerformanceImpact { get; }
    ProcessImpact MemoryFootPrint { get; }
    void Provide(object? input, Action<IFileValue, FileReference> pushFileValue, CancellationToken cancellationToken);
    IFileValue Provide(JsonNode fileSpecific);
    // IAsyncEnumerable<FileReference> Provide(object? input = null, CancellationToken cancellationToken = default);
    // Stream Open(JsonNode fileSpecific);
    void Test();
}