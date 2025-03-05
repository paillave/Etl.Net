using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

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

    public HttpFileValueProvider(string code, string url, string method, string body)
        : base(
            code,
            url,
            url,
            new HttpAdapterConnectionParameters { Url = url },
            new HttpAdapterProviderParameters { Method = method, Body = body }
        ) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
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
        using var httpClient = httpClientFactory.CreateClient(url);

        var response = GetResponse(connectionParameters, providerParameters, httpClient).Result;
        var filename = response.GetFileName(url);
        var content = response?.Content.ReadAsByteArrayAsync().Result;

        pushFileValue(new HttpFileValue(filename, content, url, Code, ConnectionName, Name));
    }

    protected override void Test(
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProviderParameters providerParameters
    )
    {
        using var httpClient = new HttpClient();
        GetResponse(connectionParameters, providerParameters, httpClient).Wait();
    }

    private static Task<HttpResponseMessage> GetResponse(
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProviderParameters providerParameters,
        HttpClient httpClient
    )
    {
        return providerParameters.Method switch
        {
            "Get" => httpClient.GetAsync(connectionParameters.Url),
            "Post" => httpClient.PostAsync(
                connectionParameters.Url,
                HttpRequestBodyEx.GetJsonBody(providerParameters.Body)
            ),
            _ => throw new NotImplementedException(),
        };
    }
}
