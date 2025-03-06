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
        string url,
        string method,
        List<string> headerParts,
        string connexionType,
        object? body
    )
        : base(
            code,
            name,
            url,
            new HttpAdapterConnectionParameters
            {
                Url = url,
                HeaderParts = headerParts,
                ConnexionType = connexionType,
            },
            new HttpAdapterProcessorParameters { Method = method, Body = body }
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
        using var stream = fileValue.Get(processorParameters.UseStreamCopy);

        var url = connectionParameters.Url;

        var httpClientFactory = context.DependencyResolver.Resolve<IHttpClientFactory>();
        using var httpClient =
            httpClientFactory?.CreateClient()
            ?? HttpClientFactory.CreateHttpClient(connectionParameters); // If none is provided, use the default factory

        var response = GetResponse(connectionParameters, processorParameters, httpClient).Result;
        var content = response.Content.ReadAsByteArrayAsync().Result;

        push(new HttpFileValue(connectionParameters, content, Code, ConnectionName, Name));
        return;
    }

    protected override void Test(
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProcessorParameters processorParameters
    )
    {
        // using var httpClient = new HttpClient();
        // httpClient
        //     .PostAsync(
        //         connectionParameters.Url,
        //         HttpRequestBodyEx.GetJsonBody(processorParameters.Body)
        //     )
        //     .Wait();
    }

    private static Task<HttpResponseMessage> GetResponse(
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProcessorParameters processorParameters,
        HttpClient httpClient
    )
    {
        return processorParameters.Method switch
        {
            "Get" => httpClient.GetAsync(connectionParameters.Url),
            "Post" => httpClient.PostAsync(
                connectionParameters.Url,
                HttpRequestBodyEx.GetJsonBody(processorParameters.Body)
            ),
            _ => throw new NotImplementedException(),
        };
    }
}
