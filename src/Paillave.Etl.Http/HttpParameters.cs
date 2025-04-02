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

public class HttpAdapterParametersBase
{
    public HttpMethodCustomEnum Method { get; set; } = HttpMethodCustomEnum.Get;
    public RequestFormat ResponseFormat { get; set; } = RequestFormat.Json;
    public RequestFormat RequestFormat { get; set; } = RequestFormat.Json;
    public object? Body { get; set; }
    public Dictionary<string, string>? AdditionalHeaders { get; set; }
    public Dictionary<string, string>? AdditionalParameters { get; set; }

    public HttpAdapterParametersBase() { }

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
    public HttpAdapterProviderParameters() { }

    public HttpAdapterProviderParameters(HttpAdapterProviderParameters other)
        : base(other) { }
}

public class HttpAdapterProcessorParameters : HttpAdapterParametersBase
{
    public bool UseStreamCopy { get; set; } = true;
    public bool UserResponseAsOutput { get; set; } = false;

    public HttpAdapterProcessorParameters() { }

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

public enum HttpMethodCustomEnum
{
    Get = 0,
    Post = 1,
    Delete = 2,
}

public enum RequestFormat
{
    Json,
    Xml,
    PlainTxt,
    Html,
    Jpeg,
    Png,
    Gif,
    Svg,
    WebP,
}

public class RequestFormatParser
{
    public static RequestFormat ParseRequestFormat(string requestFormat)
    {
        return requestFormat.ToLower() switch
        {
            "application/json" => RequestFormat.Json,
            "application/xml" => RequestFormat.Xml,
            "text/plain" => RequestFormat.PlainTxt,
            "text/html" => RequestFormat.Html,
            "image/jpeg" => RequestFormat.Jpeg,
            "image/png" => RequestFormat.Png,
            "image/gif" => RequestFormat.Gif,
            "image/svg+xml" => RequestFormat.Svg,
            "image/webp" => RequestFormat.WebP,
            _ => throw new NotImplementedException($"Unsupported format: {requestFormat}"),
        };
    }
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
