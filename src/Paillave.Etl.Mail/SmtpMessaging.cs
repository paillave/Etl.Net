using System;
using System.IO;
using Paillave.Etl.Core;
using System.Net.Mail;
using System.Collections.Generic;
using MimeKit;

namespace Paillave.Etl.Mail;

public class SmtpMessaging(/*IMailConnectionInfo*/ MailAdapterConnectionParameters connectionInfo, MailAdapterProcessorParameters? processorParameters = null) : IMessaging
{
    public string Name => "Smtp";
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
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(sender.Email, sender.DisplayName),
                Subject = subject,
                Body = body,
                SubjectEncoding = System.Text.Encoding.UTF8,
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
public class SmtpMessagingProvider : MessagingProviderBase<MailAdapterConnectionParameters>
{
    public override string Name => "Smtp";
    public override IMessaging GetMessaging(MailAdapterConnectionParameters configuration)
        => new SmtpMessaging(configuration);
}