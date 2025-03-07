using System.Collections.Generic;

namespace Paillave.Etl.HttpExtension;

public class HttpConnectionInfo : IHttpConnectionInfo
{
    public string Url { get; set; }
    public List<string>? AuthParameters { get; set; }
    public string AuthenticationType { get; set; } = "None";
    public int MaxAttempts { get; set; } = 5;
}
