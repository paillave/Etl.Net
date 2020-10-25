using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;
using System.Net;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Net.Mail;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
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
        private class MailFileValueProcessor : FileValueProcessorBase<MailAdapterConnectionParameters, MailAdapterProcessorParameters>
        {
            public MailFileValueProcessor(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters processorParameters)
                : base(code, name, connectionName, connectionParameters, processorParameters) { }
            public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
            public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
            protected override void Process(IFileValue fileValue, MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver)
            {
                using (var client = new SmtpClient(connectionParameters.Server, connectionParameters.PortNumber))
                {
                    if (!string.IsNullOrWhiteSpace(connectionParameters.Login))
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);
                    }
                    IFileValueWithDestinationMetadata destinationMetadata = fileValue.Metadata as IFileValueWithDestinationMetadata;
                    JObject metadataJson = fileValue.Metadata == null ? new JObject() : JObject.FromObject(fileValue.Metadata);
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(FormatText(processorParameters.From, metadataJson), FormatText(processorParameters.FromDisplayName, metadataJson));
                    mailMessage.To.Add(new MailAddress(FormatText(processorParameters.To, metadataJson), FormatText(processorParameters.ToDisplayName, metadataJson)));
                    mailMessage.Subject = FormatText(processorParameters.Subject, metadataJson);
                    var stream = fileValue.GetContent();
                    var fileExtension = Path.GetExtension(fileValue.Name);
                    if (string.IsNullOrWhiteSpace(processorParameters.Body)
                        && (string.Equals(fileExtension, ".html", StringComparison.InvariantCultureIgnoreCase)
                            || string.Equals(fileExtension, ".htm", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        mailMessage.IsBodyHtml = true;
                        var content = new StreamReader(stream).ReadToEnd();
                        mailMessage.Body = content;
                        if (string.IsNullOrWhiteSpace(mailMessage.Subject))
                            mailMessage.Subject = Path.GetFileNameWithoutExtension(fileValue.Name);
                    }
                    else
                    {
                        mailMessage.Body = FormatText(processorParameters.Body, metadataJson);
                        Attachment attachment = new Attachment(stream, fileValue.Name, MimeTypes.GetMimeType(fileValue.Name));
                        mailMessage.Attachments.Add(attachment);
                    }
                    client.Send(mailMessage);
                }
                push(fileValue);
            }
            private static string FormatText(string text, JToken metadata)
            {
                var regex = new Regex("\\{(?<name>.+?)(:(.+?))?}", RegexOptions.Singleline);
                if (string.IsNullOrWhiteSpace(text)) return text;
                var matches = regex.Matches(text);
                if (matches == null) return text;
                var tmp = matches.Select((i, idx) => new { idx, match = i.Groups["name"] }).OrderByDescending(i => i.idx).ToList();
                var textArgs = new List<object>();
                foreach (var item in tmp)
                {
                    text = $"{text.Substring(0, item.match.Index)}{item.idx}{text.Substring(item.match.Index + item.match.Length)}";
                    textArgs.Add(GetMetadataValue(metadata, new Queue<string>(item.match.Value.Split('.'))));
                }
                textArgs.Reverse();
                return string.Format(text, textArgs.ToArray());
            }
            private static object GetMetadataValue(JToken metadata, Queue<string> propertyPath)
            {
                if (propertyPath.Count() == 0) return metadata;
                var property = propertyPath.Dequeue();
                var subObject = metadata[property];
                return GetMetadataValue(subObject, propertyPath);
            }

            protected override void Test(MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters processorParameters)
            {
                using (var client = new SmtpClient(connectionParameters.Server, connectionParameters.PortNumber))
                {
                    if (!string.IsNullOrWhiteSpace(connectionParameters.Login))
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);
                    }
                    // IFileValueWithDestinationMetadata destinationMetadata = fileValue.Metadata as IFileValueWithDestinationMetadata;
                    // JObject metadataJson = JObject.FromObject(fileValue.Metadata);
                    // MailMessage mailMessage = new MailMessage();
                    // mailMessage.From = new MailAddress(FormatText(processorParameters.From, metadataJson), FormatText(processorParameters.FromDisplayName, metadataJson));
                    // mailMessage.To.Add(new MailAddress(FormatText(processorParameters.To, metadataJson), FormatText(processorParameters.ToDisplayName, metadataJson)));
                    // mailMessage.Body = FormatText(processorParameters.Body, metadataJson);
                    // mailMessage.Subject = FormatText(processorParameters.Subject, metadataJson);
                    // var stream = fileValue.GetContent();
                    // Attachment attachment = new Attachment(stream, fileValue.Name, MimeTypes.GetMimeType(fileValue.Name));
                    // mailMessage.Attachments.Add(attachment);
                    // try
                    // {
                    //     client.Send(mailMessage);
                    // }
                    // catch { }
                }
            }
        }
        protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, object inputParameters)
            => null;
        protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters outputParameters)
            => new MailFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
    }
}