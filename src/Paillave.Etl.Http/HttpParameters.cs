using System;
using System.Collections.Generic;

namespace Paillave.Etl.Http;

public class HttpAdapterConnectionParameters : IHttpConnectionInfo
{
    public required Uri Url { get; set; }
    public HttpAuthentication? Authentication { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public Dictionary<string, string>? HttpHeaders { get; set; }
}

public enum HttpMethods
{
    GET = 0,
    POST = 1,
}

public class HttpAdapterParametersBase
{
    public required HttpMethods Method { get; set; }
    public string ResponseFormat { get; set; } = "json/application";
    public string? RequestFormat { get; set; } //if null, assume it from ifilevalue.name

    // public object? Body { get; set; } // got from ifilevalue
    public Dictionary<string, string>? AdditionalHeaders { get; set; }
}

public class HttpAdapterProviderParameters : HttpAdapterParametersBase { }

public class HttpAdapterProcessorParameters : HttpAdapterParametersBase
{
    public bool UseStreamCopy { get; set; } = true;
}

public class HttpCallArgs
{
    public required HttpAdapterConnectionParameters ConnectionParameters { get; set; }
    public required HttpAdapterParametersBase AdapterParameters { get; set; }
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
