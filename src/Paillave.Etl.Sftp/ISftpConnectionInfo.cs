namespace Paillave.Etl.Sftp
{
    public interface ISftpConnectionInfo
    {
        string Server { get; set; }
        int PortNumber { get; set; }
        string Login { get; set; }
        string Password { get; set; }
        string PrivateKeyPassPhrase { get; set; }
        string PrivateKey { get; set; }
        int MaxAttempts { get; set; }
    }
}
