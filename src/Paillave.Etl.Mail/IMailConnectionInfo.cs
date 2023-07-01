namespace Paillave.Etl.Mail
{
    public interface IMailConnectionInfo
    {
        string Server { get; set; }
        int PortNumber { get; set; }
        string Login { get; set; }
        string Password { get; set; }
        bool? Ssl { get; set; }
        bool? Tls { get; set; }
        bool? TlsWhenAvailable { get; set; }
        int MaxAttempts { get; set; }
    }
}