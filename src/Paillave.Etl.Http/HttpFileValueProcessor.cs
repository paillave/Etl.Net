using System;
using System.Net.Http;
using System.Threading;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpFileValueProcessor
    : FileValueProcessorBase<HttpAdapterConnectionParameters, HttpAdapterProcessorParameters>
{
    public HttpFileValueProcessor(string code, string name, string connectionName, HttpAdapterConnectionParameters connectionParameters, HttpAdapterProcessorParameters processorParameters)
        : base(code, name, connectionName, connectionParameters, processorParameters) { }

    public HttpFileValueProcessor(string code, string name, string url, string method, object? body)
        : base(
            code, name, url,
            new HttpAdapterConnectionParameters
            {
                Url = url
            },
            new HttpAdapterProcessorParameters
            {
                Method = method,
                Body = body
            })
    { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

    protected override void Process(IFileValue fileValue, HttpAdapterConnectionParameters connectionParameters,
                                    HttpAdapterProcessorParameters processorParameters, Action<IFileValue> push,
                                    CancellationToken cancellationToken, IExecutionContext context)
    {
        using var stream = fileValue.Get(processorParameters.UseStreamCopy);

        var httpClientFactory = context.DependencyResolver.Resolve<IHttpClientFactory>();
        using var httpClient = httpClientFactory.CreateClient();

        if (processorParameters.Method is not "Post")
            throw new NotImplementedException();

        var response = httpClient.PostAsync(connectionParameters.Url, new StreamContent(stream)).Result;
        var content = response.Content.ReadAsByteArrayAsync().Result;
        var fileName = response.GetFileName(connectionParameters.Url);

        push(new HttpFileValue(fileName, content, connectionParameters.Url, Code, ConnectionName, Name));
    }

    protected override void Test(HttpAdapterConnectionParameters connectionParameters,
                                 HttpAdapterProcessorParameters processorParameters)
    {
        using var httpClient = new HttpClient();
        httpClient.PostAsync(connectionParameters.Url, HttpRequestBodyEx.GetJsonBody(processorParameters.Body)).Wait();
    }
}
