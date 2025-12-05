using System.ComponentModel.DataAnnotations;
using Paillave.Etl.Core;

namespace Paillave.Etl.GraphApi;

public class GraphApiAdapterConnectionParameters : IGraphApiConnectionInfo
{
    public string? From { get; set; }
    public string? FromDisplayName { get; set; }
    [Required]
    public string TenantId { get; set; }
    [Required]
    public string ClientId { get; set; }
    [Required]
    [Sensitive]
    public string ClientSecret { get; set; }
    [Required]
    public string UserId { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public bool SaveToSentItems { get; set; } = true;
}
public class GraphApiAdapterProcessorParameters
{
    public bool ToFromMetadata { get; set; }
    [Required]
    public string From { get; set; }
    public string FromDisplayName { get; set; }
    [Required]
    public string To { get; set; }
    public string ToDisplayName { get; set; }
    public string Body { get; set; }
    public string Subject { get; set; }
    public bool UseStreamCopy { get; set; } = true;
    public bool SaveToSentItems { get; set; } = true;
}
public class GraphApiProviderProcessorAdapter : ProviderProcessorAdapterBase<GraphApiAdapterConnectionParameters, GraphApiMailAdapterProviderParameters, GraphApiAdapterProcessorParameters>
{
    public override string Description => "Get and save files on an MAIL server via GraphAPI";
    public override string Name => "GraphApiMail";
    // https://github.com/jstedfast/MailKit
    protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, GraphApiAdapterConnectionParameters connectionParameters, GraphApiMailAdapterProviderParameters inputParameters)
        => new GraphApiMailFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);
    protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, GraphApiAdapterConnectionParameters connectionParameters, GraphApiAdapterProcessorParameters outputParameters)
        => new GraphApiMailFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
}
