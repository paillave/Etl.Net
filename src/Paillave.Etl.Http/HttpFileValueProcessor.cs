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

            var responseFormat = RequestFormatParser.ParseRequestFormat(
                response.Content.Headers.ContentType?.MediaType ?? "text/plain" // Fallback to plain text
            );

            var newProcessorParameters = new HttpAdapterProviderParameters(processorParameters)
            {
                ResponseFormat = responseFormat,
            };

            push(
                new HttpPostFileValue(
                    content,
                    fileName,
                    new HttpPostFileValueMetadata
                    {
                        ConnectionInfo = connectionParameters,
                        Parameters = newProcessorParameters,
                    }
                )
            );
            return;
        }

        push(fileValue);
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
