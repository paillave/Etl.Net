using System.Collections.Generic;

namespace Paillave.Etl.Ftp
{
    public interface IFtpConnectionInfo
    {
        string Server { get; set; }
        int PortNumber { get; set; }
        string Login { get; set; }
        string Password { get; set; }
        string FingerPrintSha1 { get; set; }
        string SerialNumber { get; set; }
        string PublicKey { get; set; }
        int MaxAttempts { get; set; }
        Dictionary<string, string> SubjectChecks { get; set; }
        Dictionary<string, string> IssuerChecks { get; set; }
    }
}
