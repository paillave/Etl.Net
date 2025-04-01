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
        pushFileValue(
            new HttpFileValue(
                connectionParameters.Url,
                connectionParameters.Url,
                Code,
                ConnectionName,
                Name,
                connectionParameters,
                providerParameters
            )
        );
    }

    protected override void Test(
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProviderParameters providerParameters
    )
    {
        using var httpClient = new HttpClient();
        HttpHelpers.GetResponse(connectionParameters, providerParameters, httpClient).Wait();
    }
}
