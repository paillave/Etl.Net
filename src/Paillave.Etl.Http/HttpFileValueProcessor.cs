using System;
using System.Net.Http;
using System.Threading;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpFileValueProcessor
    : FileValueProcessorBase<HttpAdapterConnectionParameters, HttpAdapterProcessorParameters>
{
    private HttpClient? _httpClient = null;

    public HttpFileValueProcessor(
        string code,
        string name,
        string connectionName,
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProcessorParameters processorParameters
    )
        : base(code, name, connectionName, connectionParameters, processorParameters) { }

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

        _httpClient ??= IHttpConnectionInfoEx.CreateHttpClient(
            connectionParameters,
            processorParameters.AdditionalHeaders
        );

        var response = Helpers
            .GetResponse(
                connectionParameters,
                processorParameters,
                _httpClient,
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

        push(
            new HttpFileValue(
                connectionParameters.Url,
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
