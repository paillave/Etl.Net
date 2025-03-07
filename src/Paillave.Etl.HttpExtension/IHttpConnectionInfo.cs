using System.Collections.Generic;

namespace Paillave.Etl.HttpExtension;

public interface IHttpConnectionInfo
{
    public string Url { get; set; }
    public List<string> AuthParameters { get; set; }
    public string AuthenticationType { get; set; }
    public int MaxAttempts { get; set; }
}
