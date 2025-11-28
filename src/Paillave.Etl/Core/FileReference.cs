using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;

namespace Paillave.Etl.Core;

public class FileReference
{
    private readonly string name;
    private readonly string connector;
    private readonly JsonNode? fileSpecific;

    public FileReference(string name, string connector, JsonNode? fileSpecific)
    {
        this.name = name;
        this.connector = connector;
        this.fileSpecific = fileSpecific;
    }

    public JsonNode FileSpecific => fileSpecific;
    public string Name => name;
    public string Connector => connector;
    public Stream GetContent(IFileValueConnectors connectors)
        => connectors.GetProvider(Connector).Provide(FileSpecific).GetContent();
    public void Delete(IFileValueConnectors connectors)
        => connectors.GetProvider(Connector).Provide(FileSpecific).Delete();
    public void SendToConnector(IFileValueConnectors connectors, string targetConnector, Dictionary<string, IEnumerable<Destination>>? destinations = null, JsonNode? targetMetadata = null)
    {
        var fileValue = connectors.GetProvider(Connector).Provide(FileSpecific);
        fileValue.Destinations = destinations;
        fileValue.Metadata = targetMetadata;
        connectors.GetProcessor(targetConnector).Process(fileValue, _ => { }, default);
    }
}