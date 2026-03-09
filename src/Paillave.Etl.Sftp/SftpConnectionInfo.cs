using System;
using System.Collections.Generic;
using System.IO;
using Paillave.Etl.Core;
using Renci.SshNet;

namespace Paillave.Etl.Sftp;


public class SftpConnectionInfo : ISftpConnectionInfo
{
    public required string Server { get; set; }
    public int PortNumber { get; set; } = 22;
    public string? Login { get; set; }
    [Sensitive]
    public string? Password { get; set; }
    [Sensitive]
    public string? PrivateKeyPassPhrase { get; set; }
    [Sensitive]
    public string? PrivateKey { get; set; }
    public int MaxAttempts { get; set; } = 3;
}
