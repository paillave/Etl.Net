namespace Paillave.Etl.Core;

// Adapter whose providers can only be built after the full connectors registry
// exists (e.g. composite providers that reference other already-registered providers).
// ConfigurationFileValueConnectorParser detects this interface and resolves
// these adapters in a second pass, after all regular providers are built.
public interface IDeferredProviderAdapter
{
    IFileValueProvider CreateDeferredProvider(
        string code, string name, string connectionName,
        object inputParameters, IFileValueConnectors connectors);
}
