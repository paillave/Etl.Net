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
        var httpClient = IHttpConnectionInfoEx.CreateHttpClient(
            connectionParameters,
            providerParameters.AdditionalHeaders
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

        pushFileValue(
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
        HttpAdapterProviderParameters providerParameters
    )
    {
        using var httpClient = new HttpClient();
        Helpers.GetResponse(connectionParameters, providerParameters, httpClient).Wait();
    }
}
