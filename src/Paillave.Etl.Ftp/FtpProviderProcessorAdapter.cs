using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Paillave.Etl.Core;

namespace Paillave.Etl.Ftp;

public class FtpAdapterConnectionParameters : IFtpConnectionInfo
{
    public string RootFolder { get; set; }
    [Required]
    public string Server { get; set; }
    public int PortNumber { get; set; } = 21;
    [Required]
    public string Login { get; set; }
    [Required]
    [Sensitive]
    public string Password { get; set; }
    [Sensitive]
    public string FingerPrintSha1 { get; set; }
    [Sensitive]
    public string SerialNumber { get; set; }
    public Dictionary<string, string> SubjectChecks { get; set; }
    public Dictionary<string, string> IssuerChecks { get; set; }
    [Sensitive]
    public string PublicKey { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public bool? Ssl { get; set; }
    public bool? Tls { get; set; }
    public bool? NoCheck { get; set; }
}
public class FtpAdapterProviderParameters
{
    public string SubFolder { get; set; }
    public string FileNamePattern { get; set; }
    public bool Recursive { get; set; }
}
public class FtpAdapterProcessorParameters
{
    public string SubFolder { get; set; }
    public bool UseStreamCopy { get; set; } = true;
    public bool BuildMissingSubFolders { get; set; } = false;
}
public class FtpProviderProcessorAdapter : ProviderProcessorAdapterBase<FtpAdapterConnectionParameters, FtpAdapterProviderParameters, FtpAdapterProcessorParameters>
{
    public override string Description => "Get and save files on an FTP server";
    public override string Name => "Ftp";
    protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters inputParameters)
        => new FtpFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);
    protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProcessorParameters outputParameters)
        => new FtpFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
}