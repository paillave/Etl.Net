using System;
using System.IO;
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

        var httpClient = IHttpConnectionInfoEx.CreateHttpClient(
            connectionParameters,
            processorParameters
        );

        var response = HttpHelpers
            .GetResponse(connectionParameters, processorParameters, httpClient)
            .Result;

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Error for {Name}  -->  {response.StatusCode}  -  {response.ReasonPhrase}"
            );
        }

        if (processorParameters.UserResponseAsOutput)
        {
            // var content = response.Content.ReadAsByteArrayAsync().Result;
            var content = new MemoryStream(
                response.Content.ReadAsByteArrayAsync().Result ?? Array.Empty<byte>()
            );
            var fileName = response.GetFileName(connectionParameters.Url);

            push(
                new HttpPostFileValue(
                    content,
                    fileName,
                    new HttpPostFileValueMetadata
                    {
                        Url = connectionParameters.Url,
                        ConnectorCode = Code,
                        ConnectionName = ConnectionName,
                        ConnectorName = Name,
                    }
                )
            );
            return;
        }

        push(fileValue);

        // return new MemoryStream(
        //     response.Content.ReadAsByteArrayAsync().Result ?? Array.Empty<byte>()
        // );


        // 1) submit the content
        // 2) if useResponseAsOutput
        //    ==>  return FileValue.Create( ... )  (nouveau type, contient metadata qui tracent la response)    (m'inspirer de InMemoryFileValue)
        //    else
        //    return le IFileValue en entrée
    }

    protected override void Test(
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProcessorParameters processorParameters
    )
    {
        using var httpClient = new HttpClient();
        HttpHelpers.GetResponse(connectionParameters, processorParameters, httpClient).Wait();
    }
}
