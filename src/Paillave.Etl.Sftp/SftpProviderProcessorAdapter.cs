using System.ComponentModel.DataAnnotations;
using System.Linq;
using Paillave.Etl.Connector;
using System.Net;

namespace Paillave.Etl.Sftp
{
    public class SftpAdapterConnectionParameters : ISftpConnectionInfo
    {
        public string RootFolder { get; set; }
        [Required]
        public string Server { get; set; }
        public int PortNumber { get; set; } = 22;
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        public string PrivateKeyPassPhrase { get; set; }
        public string PrivateKey { get; set; }
    }
    public class SftpAdapterProviderParameters
    {
        public string SubFolder { get; set; }
        public string FileNamePattern { get; set; }
    }
    public class SftpAdapterProcessorParameters
    {
        public string SubFolder { get; set; }
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
}