using System;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet;

namespace Paillave.Etl.Sftp
{

    public class SftpConnectionInfo : ISftpConnectionInfo
    {
        public string Server { get; set; }
        public int PortNumber { get; set; } = 22;
        public string Login { get; set; }
        public string Password { get; set; }
        public string PrivateKeyPassPhrase { get; set; }
        public string PrivateKey { get; set; }
    }
}
