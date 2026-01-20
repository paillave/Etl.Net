using MailKit.Net.Imap;
using MailKit.Net.Smtp;

namespace Paillave.Etl.Mail;

public static class MailAdapterConnectionParametersEx
{
    public static ImapClient CreateIMapClient(this IMailConnectionInfo connectionInfo)
    {
        var portNumber = connectionInfo.PortNumber == 0
            ? (connectionInfo.Ssl != null && connectionInfo.Ssl.Value) ? 993 : 143
            : connectionInfo.PortNumber;
        var client = new ImapClient();
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        if (connectionInfo.Ssl != null)
        {
            client.Connect(connectionInfo.Server, portNumber, connectionInfo.Ssl.Value);
        }
        else if (connectionInfo.Tls ?? false)
        {
            client.Connect(connectionInfo.Server, portNumber, MailKit.Security.SecureSocketOptions.StartTls);
        }
        else if (connectionInfo.TlsWhenAvailable ?? false)
        {
            client.Connect(connectionInfo.Server, portNumber, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);
        }
        else
        {
            client.Connect(connectionInfo.Server, portNumber);
        }
        if (!string.IsNullOrWhiteSpace(connectionInfo.Login))
            client.Authenticate(connectionInfo.Login, connectionInfo.Password);
        return client;
    }
    public static SmtpClient CreateSmtpClient(this IMailConnectionInfo connectionInfo)
    {
        var portNumber = connectionInfo.PortNumber == 0
            ? (connectionInfo.Ssl != null && connectionInfo.Ssl.Value) ? 993 : 143
            : connectionInfo.PortNumber;
        var client = new SmtpClient();
        if (connectionInfo.Ssl != null)
        {
            client.Connect(connectionInfo.Server, portNumber, connectionInfo.Ssl.Value);
        }
        else if (connectionInfo.Tls ?? false)
        {
            client.Connect(connectionInfo.Server, portNumber, MailKit.Security.SecureSocketOptions.StartTls);
        }
        else if (connectionInfo.TlsWhenAvailable ?? false)
        {
            client.Connect(connectionInfo.Server, portNumber, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);
        }
        else
        {
            client.Connect(connectionInfo.Server, portNumber);
        }
        if (!string.IsNullOrWhiteSpace(connectionInfo.Login))
            client.Authenticate(connectionInfo.Login, connectionInfo.Password);
        return client;
    }
}