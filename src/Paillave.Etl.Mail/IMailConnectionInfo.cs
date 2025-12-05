namespace Paillave.Etl.Mail
{
    public interface IMailConnectionInfo
    {
        public string? From { get; set; }
        public string? FromDisplayName { get; set; }
        string Server { get; set; }
        int PortNumber { get; set; }
        string? Login { get; set; }
        string? Password { get; set; }
        bool? Ssl { get; set; }
        bool? Tls { get; set; }
        bool? TlsWhenAvailable { get; set; }
        int MaxAttempts { get; set; }
    }
}