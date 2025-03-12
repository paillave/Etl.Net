using System;
using System.Collections.Generic;
using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpFileValue : FileValueBase<HttpFileValueMetadata>
{
    public override string Name { get; }
    private readonly IHttpConnectionInfo? _connectionInfo;
    private readonly HttpAdapterParametersBase _parameters;

    public HttpFileValue(
        string name,
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
    }

    public override StreamWithResource OpenContent() => new(GetContent());

    public override Stream GetContent() =>
        ActionRunner.TryExecute(_connectionInfo?.MaxAttempts ?? 1, GetContentSingleTime);

    private Stream GetContentSingleTime()
    {
        return new MemoryStream(_content ?? Array.Empty<byte>());
    }

    protected override void DeleteFile() { 
//use DELETE method instead of the original one
    }
}

public class HttpFileValueMetadata : FileValueMetadataBase
{
    public required string Url { get; set; }
    public List<string>? AuthParameters { get; set; }
    public string? AuthenticationType { get; set; }
}