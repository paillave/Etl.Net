using System;
using System.Collections.Generic;

namespace Paillave.Etl.Http;

public interface IHttpConnectionInfo
{
    public Uri Url { get; set; }
    public HttpAuthentication? Authentication { get; set; }
    public int MaxAttempts { get; set; }
    public Dictionary<string, string>? HttpHeaders { get; set; }
}
