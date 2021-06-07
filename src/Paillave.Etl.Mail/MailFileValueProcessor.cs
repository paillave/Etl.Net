using System;
using System.IO;
using System.Linq;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
// using HtmlAgilityPack;

namespace Paillave.Etl.Mail
{
    public class MailFileValueProcessor : FileValueProcessorBase<MailAdapterConnectionParameters, MailAdapterProcessorParameters>
    {
        public MailFileValueProcessor(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters processorParameters)
            : base(code, name, connectionName, connectionParameters, processorParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Process(
            IFileValue fileValue, MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters processorParameters,
            Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var portNumber = connectionParameters.PortNumber == 0 ? 25 : connectionParameters.PortNumber;


            IFileValueWithDestinationMetadata destinationMetadata = fileValue.Metadata as IFileValueWithDestinationMetadata;
            JObject metadataJson = fileValue.Metadata == null ? new JObject() : JObject.FromObject(fileValue.Metadata);
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(FormatText(processorParameters.From, metadataJson), FormatText(processorParameters.FromDisplayName, metadataJson));
            var mailAddresses = processorParameters.To.Split(";");
            var displayNames = (processorParameters.ToDisplayName ?? "").Split(";");

            for (int i = 0; i < mailAddresses.Length; i++)
            {
                var mailAddress = FormatText(mailAddresses[i], metadataJson);
                if (displayNames.Length == mailAddresses.Length)
                {
                    mailMessage.To.Add(new MailAddress(mailAddress, FormatText(displayNames[i], metadataJson)));
                }
                else
                {
                    mailMessage.To.Add(new MailAddress(mailAddress));
                }
            }
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
            ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => SendSingleFile(connectionParameters, mailMessage, portNumber));
            push(fileValue);
        }
        private void SendSingleFile(MailAdapterConnectionParameters connectionParameters, MailMessage mailMessage, int portNumber)
        {
            using (var client = new SmtpClient(connectionParameters.Server, portNumber))
            {
                if (!string.IsNullOrWhiteSpace(connectionParameters.Login))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);
                }
                client.Send(mailMessage);
            }
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
            var portNumber = connectionParameters.PortNumber == 0 ? 25 : connectionParameters.PortNumber;
            using (var client = new SmtpClient(connectionParameters.Server, portNumber))
            {
                if (!string.IsNullOrWhiteSpace(connectionParameters.Login))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);
                }
            }
        }
    }
}