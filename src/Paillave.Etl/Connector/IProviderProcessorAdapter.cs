using System;

namespace Paillave.Etl.Connector
{
    public interface IProviderProcessorAdapter
    {
        string Name { get; }
        string Description { get; }
        Type ConnectionParametersType { get; }
        Type ProviderParametersType { get; }
        Type ProcessorParametersType { get; }
        IFileValueProvider CreateProvider(string code, string name, string connectionName, object connectionParameters, object inputParameters);
        IFileValueProcessor CreateProcessor(string code, string name, string connectionName, object connectionParameters, object outputParameters);
    }
}