using System.ComponentModel.DataAnnotations;
using System.Linq;
using Paillave.Etl.Core;
using System.Net;

namespace Paillave.Etl.Sftp;

public class SftpAdapterConnectionParameters : ISftpConnectionInfo
{
    public string RootFolder { get; set; }
    [Required]
    public string Server { get; set; }
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
public class SftpAdapterProviderParameters
{
    public string? SubFolder { get; set; }
    public string? FileNamePattern { get; set; }
}
public class SftpAdapterProcessorParameters
{
    public string SubFolder { get; set; }
    public bool UseStreamCopy { get; set; } = true;
}
public class SftpProviderProcessorAdapter : ProviderProcessorAdapterBase<SftpAdapterConnectionParameters, SftpAdapterProviderParameters, SftpAdapterProcessorParameters>
{
    public override string Description => "Get and save files on an SFTP server";
    public override string Name => "Sftp";
    protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters inputParameters)
        => new SftpFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);
    protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProcessorParameters outputParameters)
        => new SftpFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
}