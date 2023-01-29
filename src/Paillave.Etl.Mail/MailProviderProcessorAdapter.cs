using System.ComponentModel.DataAnnotations;
using Paillave.Etl.Core;
// using HtmlAgilityPack;

namespace Paillave.Etl.Mail
{
    public class MailAdapterConnectionParameters : IMailConnectionInfo
    {
        [Required]
        public string Server { get; set; }
        public int PortNumber { get; set; } = 25;
        public string Login { get; set; }
        public string Password { get; set; }
        public bool? Ssl { get; set; }
        public int MaxAttempts { get; set; } = 3;
        public bool? Tls { get; set; }
        public bool? TlsWhenAvailable { get; set; }
    }
    public class MailAdapterProcessorParameters
    {
        public bool ToFromMetadata { get; set; }
        [Required]
        public string From { get; set; }
        public string FromDisplayName { get; set; }
        [Required]
        public string To { get; set; }
        public string ToDisplayName { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public bool UseStreamCopy { get; set; } = true;
    }
    public class MailProviderProcessorAdapter : ProviderProcessorAdapterBase<MailAdapterConnectionParameters, MailAdapterProviderParameters, MailAdapterProcessorParameters>
    {
        public override string Description => "Get and save files on an MAIL server";
        public override string Name => "Mail";
        // https://github.com/jstedfast/MailKit
        protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, MailAdapterProviderParameters inputParameters)
            => new MailFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);
        protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters outputParameters)
            => new MailFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
    }
}