using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace Paillave.Etl.HttpExtension;

public class HttpFileValueProvider
    : FileValueProviderBase<HttpAdapterConnectionParameters, HttpAdapterProviderParameters>
{
    public HttpFileValueProvider(
        string code,
        string name,
        string connectionName,
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProviderParameters providerParameters
    )
        : base(code, name, connectionName, connectionParameters, providerParameters) { }

    public HttpFileValueProvider(
        string code,
        string name,
        string connectionName,
        string url,
        string method,
        string slug,
        string responseFormat,
        string requestFormat,
        List<string> authParameters,
        List<KeyValuePair<string, string>> additionalHeaders,
        List<KeyValuePair<string, string>> additionalParameters,
        string AuthenticationType,
        object? body
    )
        : base(
            code,
            name,
            connectionName,
            new HttpAdapterConnectionParameters
            {
                Url = url,
                AuthParameters = authParameters,
                AuthenticationType = AuthenticationType,
            },
            new HttpAdapterProviderParameters
            {
                Method = method,
                Slug = slug,
                ResponseFormat = responseFormat,
                RequestFormat = requestFormat,
                Body = body,
                AdditionalHeaders = additionalHeaders,
                AdditionalParameters = additionalParameters,
            }
        ) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

    protected override void Provide(
        Action<IFileValue> pushFileValue,
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProviderParameters providerParameters,
        CancellationToken cancellationToken,
        IExecutionContext context
    )
    {
        var httpClientFactory = context.DependencyResolver.Resolve<IHttpClientFactory>();
        using var httpClient =
            httpClientFactory?.CreateClient()
            ?? HttpClientFactory.CreateHttpClient(connectionParameters, providerParameters); // If none is provided, use the default factory

        var response = Helpers
            .GetResponse(connectionParameters, providerParameters, httpClient)
            .Result;
        var content = response?.Content.ReadAsByteArrayAsync().Result;

        pushFileValue(
            new HttpFileValue(Name, content, Code, Name, ConnectionName, connectionParameters)
        );
    }

    protected override void Test(
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProviderParameters providerParameters
    )
    {
        using var httpClient = new HttpClient();
        Helpers.GetResponse(connectionParameters, providerParameters, httpClient).Wait();
    }
}
