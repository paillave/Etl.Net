using System;
using System.Collections.Generic;
using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpFileValue : FileValueBase<HttpFileValueMetadata>
{
    public override string Name { get; }
    private byte[]? _content { get; }
    private readonly IHttpConnectionInfo? _connectionInfo;

    public HttpFileValue(
        string name,
        byte[]? content,
        string url,
        string connectorCode,
        string connectionName,
        string connectorName,
        IHttpConnectionInfo? connectionInfo = null
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
        _connectionInfo = connectionInfo;
        _content = content;
    }

    public override StreamWithResource OpenContent() => new(GetContent());

    public override Stream GetContent() =>
        ActionRunner.TryExecute(_connectionInfo?.MaxAttempts ?? 1, GetContentSingleTime);

    private Stream GetContentSingleTime()
    {
        return new MemoryStream(_content ?? Array.Empty<byte>());
    }

    protected override void DeleteFile() { }
}

public class HttpFileValueMetadata : FileValueMetadataBase
{
    public required string Url { get; set; }
    public List<string>? AuthParameters { get; set; }
    public string? AuthenticationType { get; set; }
}
