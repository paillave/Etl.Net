using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Paillave.Etl.Core;


namespace Paillave.Etl.HttpExtension;
{
    public class HttpAdapterConnectionParameters : IHttpConnectionInfo
    {
        public string RootFolder { get; set; }

        [Required]
        public string Server { get; set; }
        public int PortNumber { get; set; } = 21;

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }
        public string FingerPrintSha1 { get; set; }
        public string SerialNumber { get; set; }
        public Dictionary<string, string> SubjectChecks { get; set; }
        public Dictionary<string, string> IssuerChecks { get; set; }
        public string PublicKey { get; set; }
        public int MaxAttempts { get; set; } = 3;
        public bool? Ssl { get; set; }
        public bool? Tls { get; set; }
        public bool? NoCheck { get; set; }
    }

    public class HttpAdapterProviderParameters
    {
        public string SubFolder { get; set; }
        public string FileNamePattern { get; set; }
        public bool Recursive { get; set; }
    }

    public class HttpAdapterProcessorParameters
    {
        public string SubFolder { get; set; }
        public bool UseStreamCopy { get; set; } = true;
        public bool BuildMissingSubFolders { get; set; } = false;
    }

    public class HttpProviderProcessorAdapter
        : ProviderProcessorAdapterBase<
            HttpAdapterConnectionParameters,
            HttpAdapterProviderParameters,
            HttpAdapterProcessorParameters
        >
    {
        public override string Description => "Get and save files on an Http server";
        public override string Name => "Http";

        protected override IFileValueProvider CreateProvider(
            string code,
            string name,
            string connectionName,
            HttpAdapterConnectionParameters connectionParameters,
            HttpAdapterProviderParameters inputParameters
        ) =>
            new HttpFileValueProvider(
                code,
                name,
                connectionName,
                connectionParameters,
                inputParameters
            );

        protected override IFileValueProcessor CreateProcessor(
            string code,
            string name,
            string connectionName,
            HttpAdapterConnectionParameters connectionParameters,
            HttpAdapterProcessorParameters outputParameters
        ) =>
            new HttpFileValueProcessor(
                code,
                name,
                connectionName,
                connectionParameters,
                outputParameters
            );
    }
}
