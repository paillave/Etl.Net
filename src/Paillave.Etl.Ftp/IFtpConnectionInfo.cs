namespace Paillave.Etl.Ftp
{
    public interface IFtpConnectionInfo
    {
        string Server { get; set; }
        int PortNumber { get; set; }
        string Login { get; set; }
        string Password { get; set; }
    }
}
