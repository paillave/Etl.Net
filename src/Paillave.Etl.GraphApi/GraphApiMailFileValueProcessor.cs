using System;
using System.IO;
using System.Linq;
using System.Threading;
using Paillave.Etl.Core;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Graph.Models;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Paillave.Etl.GraphApi;

public partial class GraphApiMailFileValueProcessor(string code, string name, string connectionName, GraphApiAdapterConnectionParameters connectionParameters, GraphApiAdapterProcessorParameters processorParameters) : FileValueProcessorBase<GraphApiAdapterConnectionParameters, GraphApiAdapterProcessorParameters>(code, name, connectionName, connectionParameters, processorParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    private static IEnumerable<(Destination, JsonNode)> GetDestinations(GraphApiAdapterProcessorParameters processorParameters, IFileValue fileValue)
    {
        if (processorParameters.ToFromMetadata)
        {
            var tos = processorParameters.To.Split(";")
                .SelectMany(to => (fileValue.Destinations ?? new Dictionary<string, IEnumerable<Destination>>(StringComparer.InvariantCultureIgnoreCase))
                    .TryGetValue(to, out var destinations)
                        ? destinations ?? []
                        : [])
                .ToList();
            foreach (var to in tos)
            {
                var metadataJson = fileValue.Metadata?.DeepClone() ?? new JsonObject();
                metadataJson["Destination"] = JsonSerializer.SerializeToNode(to);
                yield return (to, metadataJson);
            }
        }
        else
        {
            var tos = processorParameters.To.Split(";");
            var toNames = (processorParameters.ToDisplayName ?? "").Split(";");
            var metadataJson = fileValue.Metadata?.DeepClone() ?? new JsonObject();
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
        var messaging = new GraphApiMessaging(connectionParameters, new MessageDestination
        {
            Email = processorParameters.From,
            DisplayName = processorParameters.FromDisplayName
        }, processorParameters.SaveToSentItems);
        using var stream = fileValue.Get(processorParameters.UseStreamCopy);
        MemoryStream ms = new();
        stream.CopyTo(ms);
        foreach (var (destination, metadataJson) in destinations)
        {
            ms.Seek(0, SeekOrigin.Begin);
            var fileExtension = Path.GetExtension(fileValue.Name);
            var subject = FormatText(processorParameters.Subject, metadataJson);
            string body;
            Dictionary<string, Stream>? attachments = null;
            if (string.IsNullOrWhiteSpace(processorParameters.Body)
                && (string.Equals(fileExtension, ".html", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(fileExtension, ".htm", StringComparison.InvariantCultureIgnoreCase)))
            {
                body = new StreamReader(ms).ReadToEnd();
                if (string.IsNullOrWhiteSpace(subject))
                    subject = Path.GetFileNameWithoutExtension(fileValue.Name);
            }
            else
            {
                body = FormatText(processorParameters.Body, metadataJson);
                attachments = new Dictionary<string, Stream> { [fileValue.Name] = ms };
            }
            messaging.Send(
                    new MessageDestination
                    {
                        Email = FormatText(processorParameters.From, metadataJson),
                        DisplayName = FormatText(processorParameters.FromDisplayName, metadataJson)
                    },
                subject,
                body,
                false,
                [new MessageDestination { Email = destination.Email, DisplayName = destination.DisplayName }],
                attachments);
        }
        push(fileValue);
    }
    private static string FormatText(string text, JsonNode metadata)
    {
        var regex = MyRegex();
        if (string.IsNullOrWhiteSpace(text)) return text;
        var matches = regex.Matches(text);
        if (matches == null) return text;
        var tmp = matches.Select((i, idx) => new { idx, match = i.Groups["name"] }).OrderByDescending(i => i.idx).ToList();
        var textArgs = new List<object>();
        foreach (var item in tmp)
        {
            text = $"{text[..item.match.Index]}{item.idx}{text[(item.match.Index + item.match.Length)..]}";
            var textArg = GetMetadataValue(metadata, new Queue<string>(item.match.Value.Split('.')));
            if (textArg is JsonValue jv)
                textArgs.Add(jv.GetValue<object>());
        }
        textArgs.Reverse();
        return string.Format(text, textArgs.ToArray());
    }
    private static JsonNode? GetMetadataValue(JsonNode? metadata, Queue<string> propertyPath)
    {
        if (metadata == null) return null;
        if (propertyPath.Count == 0) return metadata;
        var property = propertyPath.Dequeue();
        var subObject = metadata[property];
        return GetMetadataValue(subObject, propertyPath);
    }
    protected override void Test(GraphApiAdapterConnectionParameters connectionParameters, GraphApiAdapterProcessorParameters processorParameters)
    {
        using var client = connectionParameters.CreateGraphApiClient();
        client.Users[connectionParameters.UserId].MailFolders.GetAsync().GetAwaiter().GetResult();
    }
    [GeneratedRegex("\\{(?<name>.+?)(:(.+?))?}", RegexOptions.Singleline)]
    private static partial Regex MyRegex();
}


public class GraphApiMessaging(IGraphApiConnectionInfo connectionInfo, MessageDestination? defaultFrom = null, bool saveToSentItems = true) : IMessaging
{
    public string Name => "GraphApi";
    public void Send(MessageDestination? sender, string subject, string body, bool important, MessageDestination[] entities, Dictionary<string, Stream>? attachments = null)
    {
        ActionRunner.TryExecute(connectionInfo.MaxAttempts, () =>
        {
            sender ??= defaultFrom ?? throw new ArgumentNullException(nameof(sender));
            var message = new Message
            {
                Subject = subject,
                Importance = important ? Importance.High : Importance.Normal,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = body
                },
                ToRecipients = [.. entities.Select(i => new Recipient
                {
                    EmailAddress = new EmailAddress { Address = i.Email, Name = i.DisplayName }
                })],
                From = new Recipient
                {
                    EmailAddress = new EmailAddress { Address = sender.Email, Name = sender.DisplayName }
                }
            };
            if (attachments != null && attachments.Count != 0)
            {
                message.Attachments = [.. attachments.Select(i => new FileAttachment
                {
                    OdataType = "#microsoft.graph.fileAttachment",
                    ContentBytes = ToByteArray(i.Value),
                    ContentType = MimeTypes.GetMimeType(i.Key),
                    Name = i.Key
                }).Cast<Attachment>()];
            }
            using var client = connectionInfo.CreateGraphApiClient();
            client.Users[connectionInfo.UserId].SendMail.PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = saveToSentItems
            }).Wait();
        });
    }
    private static byte[] ToByteArray(Stream stream)
    {
        var originalPosition = stream.CanSeek ? stream.Position : (long?)null;
        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        if (originalPosition.HasValue)
            stream.Seek(originalPosition.Value, SeekOrigin.Begin);
        return ms.ToArray();
    }
}
