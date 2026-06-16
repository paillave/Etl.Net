using System;

namespace Paillave.Etl.Core;

public class CompositeProviderParameters
{
    public string[] ProviderCodes { get; set; } = [];
}

// Adapter that exposes only a provider (no processor).
// The provider aggregates files from other already-registered source connectors,
// identified by their codes in ProviderCodes.
public class CompositeFileValueProviderAdapter(IFileValueConnectors connectors) : IProviderProcessorAdapter
{
    public string Name => "Composite";
    public string Description => "Aggregates files from multiple already-registered source connectors";

    // No connection parameters — composites reference existing connectors, not a remote service
    public Type ConnectionParametersType => typeof(object);
    public Type ProviderParametersType => typeof(CompositeProviderParameters);

    // null signals the configuration system that this adapter has no processor
    public Type? ProcessorParametersType => null;

    public IFileValueProvider CreateProvider(string code, string name, string connectionName, object connectionParameters, object inputParameters)
    {
        var parameters = (CompositeProviderParameters)inputParameters;
        return new CompositeFileValueProvider(code, connectors, parameters.ProviderCodes);
    }

    public IFileValueProcessor CreateProcessor(string code, string name, string connectionName, object connectionParameters, object outputParameters)
        => throw new NotSupportedException("Composite connector does not support file processing");
}
