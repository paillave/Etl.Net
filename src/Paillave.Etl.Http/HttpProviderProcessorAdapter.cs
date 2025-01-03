using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpAdapterConnectionParameters : IHttpConnectionInfo
{
    public required string Url { get; set; }
}

public class HttpAdapterProviderParameters
{
    public required string Method { get; set; }
    public object? Body { get; set; }
}

public class HttpAdapterProcessorParameters
{
    public bool UseStreamCopy { get; set; } = true;
    public required string Method { get; set; }
    public object? Body { get; set; }
}

public class HttpProviderProcessorAdapter
    : ProviderProcessorAdapterBase<HttpAdapterConnectionParameters,
                                   HttpAdapterProviderParameters,
                                   HttpAdapterProcessorParameters>
{
    public override string Description => "Get/send HTTP responses/requests";
    public override string Name => "Http";

    protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, HttpAdapterConnectionParameters connectionParameters, HttpAdapterProviderParameters inputParameters)
        => new HttpFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);

    protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, HttpAdapterConnectionParameters connectionParameters, HttpAdapterProcessorParameters outputParameters)
        => new HttpFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
}
