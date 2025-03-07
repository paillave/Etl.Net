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
        List<string> headerParts,
        string connexionType,
        object? body
    )
        : base(
            code,
            name,
            connectionName,
            new HttpAdapterConnectionParameters
            {
                Url = url,
                HeaderParts = headerParts,
                ConnexionType = connexionType,
            },
            new HttpAdapterProviderParameters
            {
                Method = method,
                Slug = slug,
                ResponseFormat = responseFormat,
                RequestFormat = requestFormat,
                Body = body,
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
        var url = connectionParameters.Url;

        var httpClientFactory = context.DependencyResolver.Resolve<IHttpClientFactory>();
        using var httpClient =
            httpClientFactory?.CreateClient()
            ?? HttpClientFactory.CreateHttpClient(connectionParameters); // If none is provided, use the default factory

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
