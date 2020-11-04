using System.ComponentModel.DataAnnotations;
using Paillave.Etl.Connector;

namespace Paillave.Etl.Ftp
{
    public class FtpAdapterConnectionParameters : IFtpConnectionInfo
    {
        public string RootFolder { get; set; }
        [Required]
        public string Server { get; set; }
        public int PortNumber { get; set; } = 21;
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
    }
    public class FtpAdapterProviderParameters
    {
        public string SubFolder { get; set; }
        public string FileNamePattern { get; set; }
    }
    public class FtpAdapterProcessorParameters
    {
        public string SubFolder { get; set; }
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
}