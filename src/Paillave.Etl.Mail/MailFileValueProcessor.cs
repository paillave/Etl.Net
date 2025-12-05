using System;
using System.IO;
using System.Linq;
using System.Threading;
using Paillave.Etl.Core;
using System.Net.Mail;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MimeKit;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Paillave.Etl.Mail;

public partial class MailFileValueProcessor(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters processorParameters) : FileValueProcessorBase<MailAdapterConnectionParameters, MailAdapterProcessorParameters>(code, name, connectionName, connectionParameters, processorParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    private static IEnumerable<(Destination, JsonNode)> GetDestinations(MailAdapterProcessorParameters processorParameters, IFileValue fileValue)
    {
        if (processorParameters.ToFromMetadata)
        {
            var tos = processorParameters.To.Split(";")
                .SelectMany(to => (fileValue?.Destinations ?? new Dictionary<string, IEnumerable<Destination>>(StringComparer.InvariantCultureIgnoreCase))
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
        IFileValue fileValue, MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters processorParameters,
        Action<IFileValue> push, CancellationToken cancellationToken)
    {
        var messaging = new SmtpMessaging(connectionParameters, new MessageDestination
        {
            Email = processorParameters.From,
            DisplayName = processorParameters.FromDisplayName
        });
        var destinations = MailFileValueProcessor.GetDestinations(processorParameters, fileValue).ToList();
        using var stream = fileValue.Get(processorParameters.UseStreamCopy);
        var ms = new MemoryStream();
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
        return string.Format(text, [.. textArgs]);
    }
    private static JsonNode? GetMetadataValue(JsonNode? metadata, Queue<string> propertyPath)
    {
        if (metadata == null) return null;
        if (propertyPath.Count == 0) return metadata;
        var property = propertyPath.Dequeue();
        var subObject = metadata[property];
        return GetMetadataValue(subObject, propertyPath);
    }
    protected override void Test(MailAdapterConnectionParameters connectionParameters, MailAdapterProcessorParameters processorParameters)
    {
        using var client = connectionParameters.CreateSmtpClient();
    }

    [GeneratedRegex("\\{(?<name>.+?)(:(.+?))?}", RegexOptions.Singleline)]
    private static partial Regex MyRegex();
}

public class SmtpMessaging(IMailConnectionInfo connectionInfo, MessageDestination? defaultFrom = null) : IMessaging
{
    public string Name => "Smtp";
    public void Send(MessageDestination? sender, string subject, string body, bool important, MessageDestination[] entities, Dictionary<string, Stream>? attachments = null)
    {
        ActionRunner.TryExecute(connectionInfo.MaxAttempts, () =>
        {
            sender ??= defaultFrom ?? throw new ArgumentNullException(nameof(sender));
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(sender.Email, sender.DisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                Priority = important ? MailPriority.High : MailPriority.Normal
            };
            foreach (var entity in entities)
                mailMessage.To.Add(new MailAddress(entity.Email, entity.DisplayName));
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var ms = CopyStream(attachment.Value);
                    mailMessage.Attachments.Add(new Attachment(ms, attachment.Key, MimeTypes.GetMimeType(attachment.Key)));
                }
            }
            using var smtpClient = connectionInfo.CreateSmtpClient();
            smtpClient.Send((MimeMessage)mailMessage);
        });
    }
    private static MemoryStream CopyStream(Stream stream)
    {
        var originalPosition = stream.CanSeek ? stream.Position : (long?)null;
        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);
        var ms = new MemoryStream();
        stream.CopyTo(ms);
        if (originalPosition.HasValue)
            stream.Seek(originalPosition.Value, SeekOrigin.Begin);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
}
