using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Paillave.Etl.Core;

public class FileReference(string name, string connector, string fileSpecific)
{
    public string FileSpecific { get; } = fileSpecific;
    public string Name { get; } = name;
    public string Connector { get; } = connector;

    public Stream GetContent(IFileValueConnectors connectors)
        => connectors.GetProvider(Connector).Provide(FileSpecific).GetContent();
    public void Delete(IFileValueConnectors connectors)
        => connectors.GetProvider(Connector).Provide(FileSpecific).Delete();
    public void SendToConnector(IFileValueConnectors connectors, string targetConnector, Dictionary<string, IEnumerable<Destination>>? destinations = null, object? metadata = null)
    {
        var fileValue = connectors.GetProvider(Connector).Provide(FileSpecific);
        fileValue.Destinations = destinations;
        fileValue.Metadata = metadata;
        connectors.GetProcessor(targetConnector).Process(fileValue, _ => { }, default);
    }
}