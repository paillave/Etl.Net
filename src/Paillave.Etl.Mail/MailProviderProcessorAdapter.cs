using System.ComponentModel.DataAnnotations;
using Paillave.Etl.Connector;
using Microsoft.Extensions.FileSystemGlobbing;
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
        // public bool UseIMap { get; set; }
    }
    public class MailAdapterProcessorParameters
    {
        [Required]
        public string From { get; set; }
        public string FromDisplayName { get; set; }
        [Required]
        public string To { get; set; }
        public string ToDisplayName { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }
    public class MailProviderProcessorAdapter : ProviderProcessorAdapterBase<MailAdapterConnectionParameters, object, MailAdapterProcessorParameters>
    {
        public override string Description => "Get and save files on an MAIL server";
        public override string Name => "Mail";
        // https://github.com/jstedfast/MailKit
        protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, object inputParameters)
            => null;
        protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters outputParameters)
            => new MailFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
    }
}