using System.Collections.Generic;

namespace Paillave.Etl.Ftp
{

    public class FtpConnectionInfo : IFtpConnectionInfo
    {
        public string Server { get; set; }
        public int PortNumber { get; set; } = 21;
        public string Login { get; set; }
        public string Password { get; set; }
        public string FingerPrintSha1 { get; set; }
        public string SerialNumber { get; set; }
        public string PublicKey { get; set; }
        public Dictionary<string, string> SubjectChecks { get; set; }
        public Dictionary<string, string> IssuerChecks { get; set; }
        public int MaxAttempts { get; set; } = 3;
    }
}
