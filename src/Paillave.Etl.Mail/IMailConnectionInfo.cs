namespace Paillave.Etl.Mail
{
    public interface IMailConnectionInfo
    {
        string Server { get; set; }
        int PortNumber { get; set; }
        string Login { get; set; }
        string Password { get; set; }
        bool Secured { get; set; }
        int MaxAttempts { get; set; }
    }
}