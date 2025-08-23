using System.IO;

namespace Paillave.Etl.Core;

public class FileReference(string name, string connector, string fileSpecific)
{
    public string FileSpecific { get; } = fileSpecific;
    public string Name { get; } = name;
    public string Connector { get; } = connector;

    public Stream GetContent(IFileValueConnectors connectors)
        => connectors.GetProvider(Connector).Provide(Name, FileSpecific).GetContent();
    public void Delete(IFileValueConnectors connectors)
        => connectors.GetProvider(Connector).Provide(Name, FileSpecific).Delete();
}