using Microsoft.Graph.Users.Item.MailFolders.Item.Messages;

namespace Paillave.Etl.GraphApi.Requesting;

public class AttachmentsRequestBuilderGetBuilder
{
    internal Microsoft.Graph.Users.Item.MailFolders.Item.Messages.Item.Attachments.AttachmentsRequestBuilder AttachmentsRequestBuilder;
    public string? Filter { get; set; }
    public string[]? Select { get; set; }
    public string[]? Expand { get; set; }
}
