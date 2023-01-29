using System;
using System.IO;
using System.Linq;
using System.Threading;
using Paillave.Etl.Core;
using System.Net.Mail;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using MimeKit;

namespace Paillave.Etl.Mail
{
    public class MailFileValueProcessor : FileValueProcessorBase<MailAdapterConnectionParameters, MailAdapterProcessorParameters>
    {
        public MailFileValueProcessor(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters processorParameters)
            : base(code, name, connectionName, connectionParameters, processorParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        private IEnumerable<(Destination, JObject)> GetDestinations(MailAdapterProcessorParameters processorParameters, IFileValueMetadata metadata)
        {
            if (processorParameters.ToFromMetadata)
            {
                IFileValueWithDestinationMetadata destinationMetadata = metadata as IFileValueWithDestinationMetadata;
                var tos = processorParameters.To.Split(";")
                    .SelectMany(to => (destinationMetadata?.Destinations ?? new Dictionary<string, IEnumerable<Destination>>(StringComparer.InvariantCultureIgnoreCase))
                        .TryGetValue(to, out var destinations)
                            ? destinations ?? new List<Destination>()
                            : new List<Destination>())
                    .ToList();
                foreach (var to in tos)
                {
                    JObject metadataJson = metadata == null ? new JObject() : JObject.FromObject(metadata);
                    metadataJson["Destination"] = JObject.FromObject(to);
                    yield return (to, metadataJson);
                }
            }
            else
            {
                var tos = processorParameters.To.Split(";");
                var toNames = (processorParameters.ToDisplayName ?? "").Split(";");
                JObject metadataJson = metadata == null ? new JObject() : JObject.FromObject(metadata);
                for (int i = 0; i < tos.Length; i++)
                {
                    var mailAddress = FormatText(tos[i], metadataJson);
                    if (toNames.Length == tos.Length)
                    {
                        yield return (new Destination
                        {
                            Email = mailAddress,
                            DisplayName = FormatText(toNames[i], metadataJson)
                        }, metadataJson);
                    }
                    else
                    {
                        yield return (new Destination
                        {
                            Email = mailAddress
                        }, metadataJson);
                    }
                }
            }

        }
        protected override void Process(
            IFileValue fileValue, MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters processorParameters,
            Action<IFileValue> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            var portNumber = connectionParameters.PortNumber == 0 ? 25 : connectionParameters.PortNumber;

            var destinations = GetDestinations(processorParameters, fileValue.Metadata).ToList();
            using var stream = fileValue.Get(processorParameters.UseStreamCopy);
            context.AddUnderlyingDisposables(stream);
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            foreach (var (destination, metadataJson) in destinations)
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.To.Add(new MailAddress(destination.Email, destination.DisplayName));
                mailMessage.From = new MailAddress(FormatText(processorParameters.From, metadataJson), FormatText(processorParameters.FromDisplayName, metadataJson));
                mailMessage.Subject = FormatText(processorParameters.Subject, metadataJson);
                mailMessage.IsBodyHtml = true;

                ms.Seek(0, SeekOrigin.Begin);
                var fileExtension = Path.GetExtension(fileValue.Name);
                if (string.IsNullOrWhiteSpace(processorParameters.Body)
                    && (string.Equals(fileExtension, ".html", StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(fileExtension, ".htm", StringComparison.InvariantCultureIgnoreCase)))
                {
                    var content = new StreamReader(ms).ReadToEnd();
                    mailMessage.Body = content;
                    if (string.IsNullOrWhiteSpace(mailMessage.Subject))
                        mailMessage.Subject = Path.GetFileNameWithoutExtension(fileValue.Name);
                }
                else
                {
                    mailMessage.Body = FormatText(processorParameters.Body, metadataJson);
                    Attachment attachment = new Attachment(ms, fileValue.Name, MimeTypes.GetMimeType(fileValue.Name));
                    mailMessage.Attachments.Add(attachment);
                }
                ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => SendSingleFile(connectionParameters, (MimeMessage)mailMessage, portNumber));
            }
            push(fileValue);
        }
        private void SendSingleFile(MailAdapterConnectionParameters connectionParameters, MimeMessage mailMessage, int portNumber)
        {
            // using (var client = new SmtpClient(connectionParameters.Server, portNumber))
            using (var smtpClient = connectionParameters.CreateSmtpClient())
            {
                smtpClient.Send(mailMessage);
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
            using (var client = connectionParameters.CreateSmtpClient()) { }
        }
    }
}