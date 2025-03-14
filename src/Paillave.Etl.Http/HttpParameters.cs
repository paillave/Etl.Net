using System;
using System.Collections.Generic;

namespace Paillave.Etl.Http;

public class HttpAdapterConnectionParameters : IHttpConnectionInfo
{
    public required string Url { get; set; }
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
    public string RequestFormat { get; set; } = "json/application";
    public object? Body { get; set; }
    public Dictionary<string, string>? AdditionalHeaders { get; set; }
    public Dictionary<string, string>? AdditionalParameters { get; set; }

    public HttpAdapterParametersBase(HttpAdapterParametersBase other)
    {
        Method = other.Method;
        ResponseFormat = other.ResponseFormat;
        RequestFormat = other.RequestFormat;
        Body = other.Body;
        AdditionalHeaders =
            other.AdditionalHeaders != null
                ? new Dictionary<string, string>(other.AdditionalHeaders)
                : null;
        AdditionalParameters =
            other.AdditionalParameters != null
                ? new Dictionary<string, string>(other.AdditionalParameters)
                : null;
    }
}

public class HttpAdapterProviderParameters : HttpAdapterParametersBase
{
    public HttpAdapterProviderParameters(HttpAdapterProviderParameters other)
        : base(other) { }
}

public class HttpAdapterProcessorParameters : HttpAdapterParametersBase
{
    public bool UseStreamCopy { get; set; } = true;

    public HttpAdapterProcessorParameters(HttpAdapterProcessorParameters other)
        : base(other)
    {
        UseStreamCopy = other.UseStreamCopy;
    }
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
