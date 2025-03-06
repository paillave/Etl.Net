using System.Collections.Generic;

namespace Paillave.Etl.HttpExtension;

public interface IHttpConnectionInfo
{
    public string Url { get; set; }
    public List<string> HeaderParts { get; set; }
    public string ConnexionType { get; set; }
}
