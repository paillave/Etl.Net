using System;

namespace Paillave.Etl.Ftp
{
    public class FtpConnectionInfo
    {
        public string Server { get; set; }
        public int PortNumber { get; set; } = 21;
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
