using System;

namespace Paillave.Etl.Core;

public class CompositeProviderParameters
{
    public string[] ProviderCodes { get; set; } = [];
}

// Adapter that exposes only a provider (no processor).
// The provider aggregates files from other already-registered source connectors,
// identified by their codes in ProviderCodes.
// Implements IDeferredProviderAdapter so the parser resolves it in a second pass,
// after all regular providers are built — avoiding the circular dependency that
// would arise from injecting IFileValueConnectors at construction time.
public class CompositeFileValueProviderAdapter : IProviderProcessorAdapter, IDeferredProviderAdapter
{
    public string Name => "Composite";
    public string Description => "Aggregates files from multiple already-registered source connectors";

    public Type? ConnectionParametersType => null;
    public Type ProviderParametersType => typeof(CompositeProviderParameters);
    public Type? ProcessorParametersType => null;

    public IFileValueProvider CreateProvider(string code, string name, string connectionName, object connectionParameters, object inputParameters)
        => throw new NotSupportedException("Composite providers must be built via IDeferredProviderAdapter.CreateDeferredProvider");

    public IFileValueProvider CreateDeferredProvider(string code, string name, string connectionName, object inputParameters, IFileValueConnectors connectors)
    {
        var parameters = (CompositeProviderParameters)inputParameters;
        return new CompositeFileValueProvider(code, connectors, parameters.ProviderCodes);
    }

    public IFileValueProcessor CreateProcessor(string code, string name, string connectionName, object connectionParameters, object outputParameters)
        => throw new NotSupportedException("Composite connector does not support file processing");
}
