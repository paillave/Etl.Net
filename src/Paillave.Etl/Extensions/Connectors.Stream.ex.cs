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
    /// <summary>
    /// Saves the file on the connector and returns the file value of the backed up version (reading it doesn't hit the source anymore).
    /// </summary>
    /// <param name="deleteFromSource">when true, the source file is deleted once it is saved on the backup connector; a failure on this deletion is ignored</param>
    public static IStream<IFileValue> ToBackupConnector(this IStream<IFileValue> stream, string name, string backupConnectorCode, bool deleteFromSource = false)
         => new BackupFileToConnectorStreamNode(name, new BackupFileToConnectorArgs
         {
             Input = stream,
             BackupConnectorCode = backupConnectorCode,
             DeleteFromSource = deleteFromSource,
         }).Output;
    public static IStream<IFileValue> ToFileValue(this IStream<FileReference> stream, string name)
    {
        var connectors = stream.SourceNode.ExecutionContext.Services.GetRequiredService<IFileValueConnectors>();
        return stream.Select(name, fileRef => connectors.GetProvider(fileRef.Connector).Provide(fileRef.FileSpecific));
    }
}
