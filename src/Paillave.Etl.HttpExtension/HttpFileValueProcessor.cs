using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace Paillave.Etl.HttpExtension;

public class HttpFileValueProcessor
    : FileValueProcessorBase<HttpAdapterConnectionParameters, HttpAdapterProcessorParameters>
{
    public HttpFileValueProcessor(
        string code,
        string name,
        string connectionName,
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProcessorParameters processorParameters
    )
        : base(code, name, connectionName, connectionParameters, processorParameters) { }

    public HttpFileValueProcessor(
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
            new HttpAdapterProcessorParameters
            {
                Method = method,
                Slug = slug,
                RequestFormat = requestFormat,
                ResponseFormat = responseFormat,
                Body = body,
                AdditionalHeaders = additionalHeaders,
                AdditionalParameters = additionalParameters,
            }
        ) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

    protected override void Process(
        IFileValue fileValue,
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProcessorParameters processorParameters,
        Action<IFileValue> push,
        CancellationToken cancellationToken,
        IExecutionContext context
    )
    {
        // using var stream = fileValue.Get(processorParameters.UseStreamCopy);

        var httpClientFactory = context.DependencyResolver.Resolve<IHttpClientFactory>();
        using var httpClient =
            httpClientFactory?.CreateClient()
            ?? HttpClientFactory.CreateHttpClient(connectionParameters, processorParameters); // If none is provided, use the default factory

        var response = Helpers
            .GetResponse(connectionParameters, processorParameters, httpClient)
            .Result;
        var content = response.Content.ReadAsByteArrayAsync().Result;

        push(new HttpFileValue(Name, content, Code, Name, ConnectionName, connectionParameters));
        return;
    }

    protected override void Test(
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProcessorParameters processorParameters
    )
    {
        using var httpClient = new HttpClient();
        Helpers.GetResponse(connectionParameters, processorParameters, httpClient).Wait();
    }
}
