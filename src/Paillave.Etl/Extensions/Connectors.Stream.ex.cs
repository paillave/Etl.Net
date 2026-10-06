using Microsoft.Extensions.DependencyInjection;

namespace Paillave.Etl.Core;

public static class ConnectorsStreamEx
{
    public static IStream<IFileValue> FromConnector<TIn>(this IStream<TIn> stream, string name, string inputConnectorCode)
        => stream.CrossApply(name, new ConnectorFileValueProvider<TIn>(stream.SourceNode.ExecutionContext.Services.GetRequiredService<IFileValueConnectors>().GetProvider(inputConnectorCode)));
    /// <param name="useNewVersion">when true, the output file value corresponds to the file that has just been saved on the connector instead of the input file value</param>
    public static IStream<IFileValue> ToConnector(this IStream<IFileValue> stream, string name, string outputConnectorCode, bool useNewVersion = false)
         => new ProcessFileToConnectorStreamNode(name, new ProcessFileToConnectorArgs
         {
             Input = stream,
             OutputConnectorCode = outputConnectorCode,
             UseNewVersion = useNewVersion,
         }).Output;
    public static IStream<IFileValue> ToFileValue(this IStream<FileReference> stream, string name)
    {
        var connectors = stream.SourceNode.ExecutionContext.Services.GetRequiredService<IFileValueConnectors>();
        return stream.Select(name, fileRef => connectors.GetProvider(fileRef.Connector).Provide(fileRef.FileSpecific));
    }
}
