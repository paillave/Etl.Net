using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpFileValue : FileValueBase<HttpFileValueMetadata>
{
    public override string Name { get; }
    private byte[]? _content { get; }

    public HttpFileValue(
        string name,
        byte[]? content,
        string url,
        string connectorCode,
        string connectionName,
        string connectorName
    )
        : base(
            new HttpFileValueMetadata
            {
                Url = url,
                ConnectorCode = connectorCode,
                ConnectionName = connectionName,
                ConnectorName = connectorName,
            }
        )
    {
        Name = name;
        _content = content;
    }

    public override Stream GetContent() => new MemoryStream(_content ?? new byte[0]);

    public override StreamWithResource OpenContent() => new(GetContent());

    protected override void DeleteFile() { }
}

public class HttpFileValueMetadata : FileValueMetadataBase
{
    public required string Url { get; set; }
}
