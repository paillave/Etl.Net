using System;
using System.Collections.Generic;
using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.HttpExtension;

public class HttpFileValue : FileValueBase<HttpFileValueMetadata>
{
    public override string Name { get; }
    private byte[]? _content { get; }
    private readonly IHttpConnectionInfo _connectionInfo;

    public HttpFileValue(IHttpConnectionInfo connectionInfo)
        : this(null, null, null, null, null, connectionInfo) { }

    public HttpFileValue(
        string name,
        byte[]? content,
        string connectorCode,
        string connectorName,
        string connectionName,
        IHttpConnectionInfo connectionInfo
    )
        : base(
            new HttpFileValueMetadata
            {
                Url = connectionInfo.Url,
                AuthParameters = connectionInfo.AuthParameters,
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
        ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);

    private Stream GetContentSingleTime()
    {
        return new MemoryStream(_content ?? Array.Empty<byte>());
    }

    protected override void DeleteFile() { }

    // protected override void DeleteFile() =>
    //     ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);

    // protected void DeleteFileSingleTime()
    // {
    //     // TODO: is there even the equivalent of a delete in the context of RESTful APIs?

    //     // var pathToDelete = StringEx.ConcatenatePath(_folder, this.Name).Replace('\\', '/');
    //     // using (HttpClient client = _connectionInfo.CreateHttpClient())
    //     //     client.DeleteFile(pathToDelete);
    // }
}

public class HttpFileValueMetadata : FileValueMetadataBase
{
    public required string Url { get; set; }
    public List<string>? AuthParameters { get; set; }
    public string AuthenticationType { get; set; } = "None";
}
