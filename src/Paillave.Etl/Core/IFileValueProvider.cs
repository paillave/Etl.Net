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
    IAsyncEnumerable<IFileValue> ProvideAsync(object? input = null, CancellationToken cancellationToken = default);
    void Provide(object? input, Action<IFileValue, FileReference> pushFileValue, CancellationToken cancellationToken);
    IFileValue Provide(JsonNode fileSpecific);
    IAsyncEnumerable<FileReference> EnumerateAsync(object? input = null, CancellationToken cancellationToken = default);
    Stream Open(JsonNode fileSpecific);
    void Test();
}