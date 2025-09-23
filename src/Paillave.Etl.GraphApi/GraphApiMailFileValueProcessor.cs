using System;
using System.IO;
using System.Linq;
using System.Threading;
using Paillave.Etl.Core;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Models;

namespace Paillave.Etl.GraphApi;

public class GraphApiMailFileValueProcessor : FileValueProcessorBase<GraphApiAdapterConnectionParameters, GraphApiAdapterProcessorParameters>
{
    public GraphApiMailFileValueProcessor(string code, string name, string connectionName, GraphApiAdapterConnectionParameters connectionParameters, GraphApiAdapterProcessorParameters processorParameters)
        : base(code, name, connectionName, connectionParameters, processorParameters) { }
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    private IEnumerable<(Destination, JObject)> GetDestinations(GraphApiAdapterProcessorParameters processorParameters, IFileValue fileValue)
    {
        if (processorParameters.ToFromMetadata)
        {
            var tos = processorParameters.To.Split(";")
                .SelectMany(to => (fileValue.Destinations ?? new Dictionary<string, IEnumerable<Destination>>(StringComparer.InvariantCultureIgnoreCase))
                    .TryGetValue(to, out var destinations)
                        ? destinations ?? []
                        : new List<Destination>())
                .ToList();
            foreach (var to in tos)
            {
                JObject metadataJson = fileValue.Metadata == null ? [] : JObject.FromObject(fileValue.Metadata);
                metadataJson["Destination"] = JObject.FromObject(to);
                yield return (to, metadataJson);
            }
        }
        else
        {
            var tos = processorParameters.To.Split(";");
            var toNames = (processorParameters.ToDisplayName ?? "").Split(";");
            JObject metadataJson = fileValue.Metadata == null ? new JObject() : JObject.FromObject(fileValue.Metadata);
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
        IFileValue fileValue, GraphApiAdapterConnectionParameters connectionParameters, GraphApiAdapterProcessorParameters processorParameters,
        Action<IFileValue> push, CancellationToken cancellationToken)
    {
        var destinations = GetDestinations(processorParameters, fileValue).ToList();
        using var stream = fileValue.Get(processorParameters.UseStreamCopy);
        MemoryStream ms = new MemoryStream();
        stream.CopyTo(ms);
        foreach (var (destination, metadataJson) in destinations)
        {
            Message mailMessage = new Message();
            mailMessage.ToRecipients = new List<Recipient>
            {
                new Recipient { EmailAddress = new EmailAddress { Address = destination.Email, Name = destination.DisplayName } }
            };
            mailMessage.From = new Recipient { EmailAddress = new EmailAddress { Address = FormatText(processorParameters.From, metadataJson), Name = FormatText(processorParameters.FromDisplayName, metadataJson) } };

            mailMessage.Subject = FormatText(processorParameters.Subject, metadataJson);

            ms.Seek(0, SeekOrigin.Begin);
            var fileExtension = Path.GetExtension(fileValue.Name);
            if (string.IsNullOrWhiteSpace(processorParameters.Body)
                && (string.Equals(fileExtension, ".html", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(fileExtension, ".htm", StringComparison.InvariantCultureIgnoreCase)))
            {
                var content = new StreamReader(ms).ReadToEnd();
                mailMessage.Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = content
                };
                if (string.IsNullOrWhiteSpace(mailMessage.Subject))
                    mailMessage.Subject = Path.GetFileNameWithoutExtension(fileValue.Name);
            }
            else
            {
                mailMessage.Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = FormatText(processorParameters.Body, metadataJson)
                };
                mailMessage.Attachments = new List<Attachment>
                {
                    new FileAttachment
                    {
                        OdataType = "#microsoft.graph.fileAttachment",
                        ContentBytes = ms.ToArray(),
                        ContentType = MimeTypes.GetMimeType(fileValue.Name),
                        Name = fileValue.Name
                    }
                };
            }
            ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => SendSingleFile(connectionParameters, mailMessage, processorParameters.SaveToSentItems));
        }
        push(fileValue);
    }
    private void SendSingleFile(GraphApiAdapterConnectionParameters connectionParameters, Message mailMessage, bool saveToSentItems)
    {
        // using (var client = new SmtpClient(connectionParameters.Server, portNumber))
        using (var graphApiClient = connectionParameters.CreateGraphApiClient())
        {
            graphApiClient.Users[connectionParameters.UserId].SendMail.PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = mailMessage,
                SaveToSentItems = true
            }).Wait();
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
    protected override void Test(GraphApiAdapterConnectionParameters connectionParameters, GraphApiAdapterProcessorParameters processorParameters)
    {
        using (var client = connectionParameters.CreateGraphApiClient())
        {
            client.Users[connectionParameters.UserId].MailFolders.GetAsync().Wait();
        }
    }
}
