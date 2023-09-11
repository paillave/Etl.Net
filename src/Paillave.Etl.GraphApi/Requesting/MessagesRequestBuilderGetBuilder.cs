using Microsoft.Graph.Users.Item.MailFolders.Item.Messages;

namespace Paillave.Etl.GraphApi.Requesting;

public class MessagesRequestBuilderGetBuilder
{
    internal MessagesRequestBuilder MessagesRequestBuilder;
    public string? Filter { get; set; }
    public string[]? Select { get; set; }
    public string[]? Expand { get; set; }
}
