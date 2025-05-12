using System.Collections.Generic;
using Paillave.Etl.Http;

public interface IHttpAdapterParameters
{
    HttpMethodCustomEnum Method { get; set; }
    RequestFormat ResponseFormat { get; set; }
    RequestFormat RequestFormat { get; set; }
    object? Body { get; set; }
    Dictionary<string, string>? AdditionalHeaders { get; set; }
    Dictionary<string, string>? AdditionalParameters { get; set; }
}
