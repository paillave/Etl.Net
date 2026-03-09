namespace Paillave.Etl.GraphApi;

public interface IGraphApiConnectionInfo
{
    public string? From { get; set; }
    public string? FromDisplayName { get; set; }
    string TenantId { get; set; }
    string ClientId { get; set; }
    string ClientSecret { get; set; }
    string UserId { get; set; }
    int MaxAttempts { get; set; }
    bool SaveToSentItems { get; set; }
}
