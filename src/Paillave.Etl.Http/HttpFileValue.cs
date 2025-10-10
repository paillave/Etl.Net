using System;
using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpFileValue : FileValueBase
{
    public override string Name { get; }
    private readonly IHttpConnectionInfo _connectionInfo;
    private readonly IHttpAdapterParameters _parameters;

    public HttpFileValue(string url, IHttpConnectionInfo? connectionInfo, IHttpAdapterParameters? parameters)
    {
        if (connectionInfo != null)
        {
            connectionInfo.Url = url;
            _connectionInfo = connectionInfo;
        }
        else
        {
            _connectionInfo = new HttpAdapterConnectionParameters { Url = url };
        }
        Name = url;
        _parameters = parameters ?? new HttpAdapterProviderParameters() { Method = HttpMethodCustomEnum.Get };
    }


    public override StreamWithResource OpenContent() => new(GetContent());

    public override Stream GetContent() =>
        ActionRunner.TryExecute(_connectionInfo?.MaxAttempts ?? 1, GetContentSingleTime);

    private Stream GetContentSingleTime()
    {
        var httpClient = IHttpConnectionInfoEx.CreateHttpClient(_connectionInfo, _parameters);

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
        var httpClient = IHttpConnectionInfoEx.CreateHttpClient(_connectionInfo, _parameters);

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
