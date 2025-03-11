using System.Collections.Generic;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpAdapterConnectionParameters : IHttpConnectionInfo
{
    public required string Url { get; set; }
    public List<string>? AuthParameters { get; set; }
    public string AuthenticationType { get; set; } = "None";
    public int MaxAttempts { get; set; } = 5;
}

public class HttpAdapterParametersBase
{
    public required string Method { get; set; }
    public string Slug { get; set; } = "/";
    public string ResponseFormat { get; set; } = "json";
    public string RequestFormat { get; set; } = "json";
    public object? Body { get; set; }
    public List<KeyValuePair<string, string>>? AdditionalHeaders { get; set; }
    public List<KeyValuePair<string, string>>? AdditionalParameters { get; set; }
}

public class HttpAdapterProviderParameters : HttpAdapterParametersBase { }

public class HttpAdapterProcessorParameters : HttpAdapterParametersBase
{
    public bool UseStreamCopy { get; set; } = true;
}

public class HttpCallArgs
{
    public HttpAdapterConnectionParameters ConnectionParameters { get; set; }
    public HttpAdapterParametersBase AdapterParameters { get; set; }
}

// public class HttpProviderProcessorAdapter
//     : ProviderProcessorAdapterBase<
//         HttpAdapterConnectionParameters,
//         HttpAdapterProviderParameters,
//         HttpAdapterProcessorParameters
//     >
// {
//     public override string Description => "Get/send HTTP responses/requests";
//     public override string Name => "Http";

//     protected override IFileValueProvider CreateProvider(
//         string code,
//         string name,
//         string connectionName,
//         HttpAdapterConnectionParameters connectionParameters,
//         HttpAdapterProviderParameters inputParameters
//     ) =>
//         new HttpFileValueProvider(
//             code,
//             name,
//             connectionName,
//             connectionParameters,
//             inputParameters
//         );

//     protected override IFileValueProcessor CreateProcessor(
//         string code,
//         string name,
//         string connectionName,
//         HttpAdapterConnectionParameters connectionParameters,
//         HttpAdapterProcessorParameters outputParameters
//     ) =>
//         new HttpFileValueProcessor(
//             code,
//             name,
//             connectionName,
//             connectionParameters,
//             outputParameters
//         );
// }
