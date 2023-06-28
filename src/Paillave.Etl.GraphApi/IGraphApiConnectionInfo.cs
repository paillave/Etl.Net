namespace Paillave.Etl.GraphApi;

public interface IGraphApiConnectionInfo
{
    string TenantId { get; set; }
    string ClientId { get; set; }
    string ClientSecret { get; set; }
    string UserId { get; set; }
    int MaxAttempts { get; set; }
}
