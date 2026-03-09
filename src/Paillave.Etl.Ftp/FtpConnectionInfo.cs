using System.Collections.Generic;
using Paillave.Etl.Core;

namespace Paillave.Etl.Ftp;


public class FtpConnectionInfo : IFtpConnectionInfo
{
    public string Server { get; set; }
    public int PortNumber { get; set; } = 21;
    public string Login { get; set; }
    [Sensitive]
    public string Password { get; set; }
    [Sensitive]
    public string FingerPrintSha1 { get; set; }
    public string SerialNumber { get; set; }
    [Sensitive]
    public string PublicKey { get; set; }
    public Dictionary<string, string> SubjectChecks { get; set; }
    public Dictionary<string, string> IssuerChecks { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public bool? Ssl { get; set; }
    public bool? Tls { get; set; }
    public bool? NoCheck { get; set; }
}
