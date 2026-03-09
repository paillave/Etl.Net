using System;
using System.IO;
using System.Linq;
using Paillave.Etl.Core;
using System.Collections.Generic;
using Microsoft.Graph.Models;

namespace Paillave.Etl.GraphApi;

public class GraphApiMessaging(/*IGraphApiConnectionInfo*/ GraphApiAdapterConnectionParameters connectionInfo, GraphApiAdapterProcessorParameters? processorParameters = null) : IMessaging
{
    public string Name => "GraphApi";
    public void Send(MessageContact? sender, string subject, string body, bool important, MessageContact[] entities, Dictionary<string, Stream>? attachments = null)
    {
        ActionRunner.TryExecute(connectionInfo.MaxAttempts, () =>
        {
            MessageContact? defaultFrom = null;
            defaultFrom ??= processorParameters?.From != null ? new MessageContact
            {
                Email = processorParameters.From,
                DisplayName = processorParameters.FromDisplayName
            } : null;
            defaultFrom ??= connectionInfo.From != null ? new MessageContact
            {
                Email = connectionInfo.From,
                DisplayName = connectionInfo.FromDisplayName
            } : null;
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
                SaveToSentItems = connectionInfo.SaveToSentItems
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
public class GraphApiMessagingProvider : MessagingProviderBase<GraphApiAdapterConnectionParameters>
{
    public override string Name => "GraphApi";
    public override IMessaging GetMessaging(GraphApiAdapterConnectionParameters configuration)
        => new GraphApiMessaging(configuration);
}