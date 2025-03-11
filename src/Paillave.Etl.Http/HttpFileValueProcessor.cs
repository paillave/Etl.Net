using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

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
            name,
            new Uri(new Uri(url.TrimEnd('/')), slug).ToString(),
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

    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
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

        var httpClientFactory = context.DependencyResolver.Resolve<HttpClientFactory>();
        using var httpClient =
            httpClientFactory?.GetClient(Name, connectionParameters, processorParameters)
            ?? HttpClientFactory.CreateClient(connectionParameters, processorParameters); // If none is provided, call static CreateClient method

        var response = Helpers
            .GetResponse(
                connectionParameters,
                processorParameters,
                httpClient,
                new StreamContent(stream)
            )
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
            processorParameters.Slug
        ).ToString();

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
