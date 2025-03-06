using System;
using System.IO;
using System.Threading;
using FluentFtp;
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

        var httpClientFactory = context.DependencyResolver.Resolve<IHttpClientFactory>();
        using var httpClient =
            httpClientFactory?.CreateClient()
            ?? HttpClientFactory.CreateHttpClient(connectionParameters); // If none is provided, use the default factory

        var response;
        if (processorParameters.Method == "GET")
        {
            response = httpClient.GetAsync(connectionParameters.Url).Result;
        }
        else if (processorParameters.Method == "POST")
        {
            response = httpClient
                .PostAsync(
                    connectionParameters.Url,
                    HttpRequestBodyEx.GetJsonBody(processorParameters.Body)
                )
                .Result;
        }
        else
        {
            throw new NotImplementedException(
                $"Method type '{connectionParameters.ConnexionType}' is not implemented."
            );
        }

        var content = response.Content.ReadAsByteArrayAsync().Result;
        var fileName = response.GetFileName(connectionParameters.Url);

        push(
            new HttpFileValue(
                fileName,
                content,
                connectionParameters.Url,
                Code,
                ConnectionName,
                Name
            )
        );
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
}
