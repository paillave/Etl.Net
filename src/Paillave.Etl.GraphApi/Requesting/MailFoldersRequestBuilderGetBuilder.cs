using Microsoft.Graph.Users.Item.MailFolders;
namespace Paillave.Etl.GraphApi.Requesting;

public class MailFoldersRequestBuilderGetBuilder
{
    internal MailFoldersRequestBuilder MailFoldersRequestBuilder;
    public string? Filter { get; set; }
    public string[]? Select { get; set; }
}