namespace Paillave.Etl.Core;
public static class ConnectorsStreamEx
{
    public static IStream<IFileValue> FromConnector<TIn>(this IStream<TIn> stream, string name, string inputConnectorCode)
        => stream.CrossApply<TIn, IFileValue>(name, new ConnectorFileValueProvider<TIn>(stream.SourceNode.ExecutionContext.Connectors.GetProvider(inputConnectorCode)));
    public static IStream<IFileValue> ToConnector(this IStream<IFileValue> stream, string name, string outputConnectorCode)
         => new ProcessFileToConnectorStreamNode(name, new ProcessFileToConnectorArgs
         {
             Input = stream,
             OutputConnectorCode = outputConnectorCode,
         }).Output;
}
