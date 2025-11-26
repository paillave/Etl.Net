using Microsoft.Extensions.DependencyInjection;

namespace Paillave.Etl.Core;

public static class ConnectorsStreamEx
{
    public static IStream<IFileValue> FromConnector<TIn>(this IStream<TIn> stream, string name, string inputConnectorCode)
        => stream.CrossApply(name, new ConnectorFileValueProvider<TIn>(stream.SourceNode.ExecutionContext.Services.GetRequiredService<IFileValueConnectors>().GetProvider(inputConnectorCode)));
    public static IStream<IFileValue> ToConnector(this IStream<IFileValue> stream, string name, string outputConnectorCode)
         => new ProcessFileToConnectorStreamNode(name, new ProcessFileToConnectorArgs
         {
             Input = stream,
             OutputConnectorCode = outputConnectorCode,
         }).Output;
}
