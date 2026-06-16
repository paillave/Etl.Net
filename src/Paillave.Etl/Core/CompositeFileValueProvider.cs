using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading;

namespace Paillave.Etl.Core;

public class CompositeFileValueProvider : IFileValueProvider
{
    private readonly IFileValueConnectors _connectors;
    private readonly IReadOnlyList<string> _providerCodes;

    public string Code { get; }

    public CompositeFileValueProvider(string code, IFileValueConnectors connectors, params string[] providerCodes)
    {
        Code = code;
        _connectors = connectors;
        _providerCodes = providerCodes;
    }

    private IEnumerable<IFileValueProvider> GetProviders()
        => _providerCodes.Select(_connectors.GetProvider);

    public ProcessImpact PerformanceImpact
        => GetProviders().Select(p => p.PerformanceImpact).DefaultIfEmpty(ProcessImpact.Light).Max();

    public ProcessImpact MemoryFootPrint
        => GetProviders().Select(p => p.MemoryFootPrint).DefaultIfEmpty(ProcessImpact.Light).Max();

    public void Provide(object? input, System.Action<IFileValue, FileReference> pushFileValue, CancellationToken cancellationToken)
    {
        foreach (var provider in GetProviders())
        {
            if (cancellationToken.IsCancellationRequested) break;
            provider.Provide(input, pushFileValue, cancellationToken);
        }
    }

    // Each underlying provider pushes its own FileReference (with its own code and fileSpecific),
    // so files can always be re-opened through the original provider. This method is never reached
    // in normal flow but must satisfy the interface.
    public IFileValue Provide(JsonNode? fileSpecific)
        => throw new System.NotSupportedException($"Composite provider '{Code}' does not support direct file re-opening by metadata");

    public Stream Open(JsonNode fileSpecific)
        => Provide(fileSpecific).GetContent();

    public void Test()
    {
        foreach (var provider in GetProviders())
            provider.Test();
    }

    public async IAsyncEnumerable<IFileValue> ProvideAsync(object? input = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var provider in GetProviders())
            await foreach (var fileValue in provider.ProvideAsync(input, cancellationToken))
                yield return fileValue;
    }

    public async IAsyncEnumerable<FileReference> EnumerateAsync(object? input = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var provider in GetProviders())
            await foreach (var fileRef in provider.EnumerateAsync(input, cancellationToken))
                yield return fileRef;
    }
}
