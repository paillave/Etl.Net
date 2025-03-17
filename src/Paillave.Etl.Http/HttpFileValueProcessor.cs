using System;
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

        var payload = fileValue.GetContent();

        push(
            new HttpFileValue(
                connectionParameters.Url,
                connectionParameters.Url,
                Code,
                ConnectionName,
                Name,
                connectionParameters,
                processorParameters
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
