using System;
using System.Collections.Generic;
using System.IO;
using Paillave.Etl.Core;
using Fluid;

namespace Paillave.Etl.Http;

public class HttpFileValue : FileValueBase<HttpFileValueMetadata>
{
    public override string Name { get; }
    private readonly IHttpConnectionInfo _connectionInfo;
    private readonly IHttpAdapterParameters _parameters;

    private readonly IFileValueMetadata? _additionalMetadata = null;

    public HttpFileValue(
        string name,
        string url,
        string connectorCode,
        string connectionName,
        string connectorName,
        IHttpConnectionInfo? connectionInfo = null,
        IHttpAdapterParameters? parameters = null,
        IFileValueMetadata? additionalMetadata = null
    )
        : base(
            new HttpFileValueMetadata
            {
                ConnectionInfo =
                    connectionInfo ?? new HttpAdapterConnectionParameters { Url = url },
                Parameters = parameters ?? new HttpAdapterProviderParameters { },
            }
        )
    {
        Name = name;
        _connectionInfo = connectionInfo ?? new HttpAdapterConnectionParameters { Url = url };
        _parameters =
            parameters ?? new HttpAdapterProviderParameters() { Method = HttpMethodCustomEnum.Get };
    }

    public override StreamWithResource OpenContent() => new(GetContent());

    public override Stream GetContent() =>
        ActionRunner.TryExecute(_connectionInfo?.MaxAttempts ?? 1, GetContentSingleTime);

    private Stream GetContentSingleTime()
    {
        var httpClient = IHttpConnectionInfoEx.CreateHttpClient(_connectionInfo, _parameters, _additionalMetadata);

        var response = HttpHelpers.GetResponse(_connectionInfo, _parameters, httpClient).Result;

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Error for {Name}  -->  StatusCode: {response.StatusCode}  -  ReasonPhrase: {response.ReasonPhrase}"
            );
        }

        return new MemoryStream(
            response.Content.ReadAsByteArrayAsync().Result ?? Array.Empty<byte>()
        );
    }

    protected override void DeleteFile()
    {
        var httpClient = IHttpConnectionInfoEx.CreateHttpClient(_connectionInfo, _parameters, _additionalMetadata);

        var parameters = new HttpAdapterProviderParameters(_parameters)
        {
            Method = HttpMethodCustomEnum.Delete,
        };

        var response = HttpHelpers.GetResponse(_connectionInfo, parameters, httpClient).Result;

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
    public required IHttpConnectionInfo ConnectionInfo { get; set; }
    public required IHttpAdapterParameters Parameters { get; set; }
}
