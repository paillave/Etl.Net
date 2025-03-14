using System;
using System.Collections.Generic;
using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpFileValue : FileValueBase<HttpFileValueMetadata>
{
    public override string Name { get; }
    private readonly IHttpConnectionInfo _connectionInfo;
    private readonly HttpAdapterParametersBase _parameters;

    public HttpFileValue(
        string name,
        string url,
        string connectorCode,
        string connectionName,
        string connectorName,
        IHttpConnectionInfo? connectionInfo = null,
        HttpAdapterParametersBase? parameters = null
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
        _connectionInfo = connectionInfo ?? new HttpAdapterConnectionParameters { Url = url };
        _parameters = parameters ?? new HttpAdapterParametersBase() { Method = HttpMethodCustomEnum.GET };
    }

    public override StreamWithResource OpenContent() => new(GetContent());

    public override Stream GetContent() =>
        ActionRunner.TryExecute(_connectionInfo?.MaxAttempts ?? 1, GetContentSingleTime);

    private Stream GetContentSingleTime()
    {
        var httpClient = IHttpConnectionInfoEx.CreateHttpClient(_connectionInfo, _parameters);

        var response = Helpers.GetResponse(_connectionInfo, _parameters, httpClient).Result;

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Error for {Name}  -->  {response.StatusCode}  -  {response.ReasonPhrase}"
            );
        }

        return new MemoryStream(
            response.Content.ReadAsByteArrayAsync().Result ?? Array.Empty<byte>()
        );
    }

    protected override void DeleteFile()
    {
        var httpClient = IHttpConnectionInfoEx.CreateHttpClient(_connectionInfo, _parameters);

        var parameters = new HttpAdapterParametersBase(_parameters) { Method = HttpMethodCustomEnum.DELETE };

        var response = Helpers.GetResponse(_connectionInfo, _parameters, httpClient).Result;

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Error for {Name}  -->  {response.StatusCode}  -  {response.ReasonPhrase}"
            );
        }
    }
}

public class HttpFileValueMetadata : FileValueMetadataBase
{
    public required string Url { get; set; }
    public List<string>? AuthParameters { get; set; }
    public string? AuthenticationType { get; set; }
}
