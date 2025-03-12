using System;
using System.Collections.Generic;
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

    public HttpFileValueProvider(
        string code,
        string url,
        string method,
        object? body = null,
        string slug = "/",
        string responseFormat = "json",
        string requestFormat = "json",
        string AuthenticationType = "None",
        List<string>? authParameters = null,
        List<KeyValuePair<string, string>>? additionalHeaders = null,
        List<KeyValuePair<string, string>>? additionalParameters = null
    )
        : base(
            code,
            new Uri(new Uri(url.TrimEnd('/')), slug).ToString(),
            new Uri(new Uri(url.TrimEnd('/')), slug).ToString(),
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
        var httpClientFactory =
            context.DependencyResolver.Resolve<HttpClientFactory>() ?? HttpClientFactory.Instance;
        var httpClient = httpClientFactory.GetClient(
            Name,
            connectionParameters,
            providerParameters
        );

        var response = Helpers
            .GetResponse(connectionParameters, providerParameters, httpClient)
            .Result;

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Error for {Name}  -->  {response.StatusCode}  -  {response.ReasonPhrase}"
            );
        }

        var content = response.Content.ReadAsByteArrayAsync().Result;

        var fileName = new Uri(
            new Uri(connectionParameters.Url.TrimEnd('/')),
            providerParameters.Slug
        ).ToString();

        pushFileValue(
            new HttpFileValue(
                fileName,
                content,
                connectionParameters.Url,
                Code,
                ConnectionName,
                Name
            )
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
