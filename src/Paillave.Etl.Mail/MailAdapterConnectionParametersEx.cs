using MailKit.Net.Imap;

namespace Paillave.Etl.Mail
{
    public static class MailAdapterConnectionParametersEx
    {
        public static ImapClient CreateIMapClient(this IMailConnectionInfo connectionInfo)
        {
            var portNumber = connectionInfo.PortNumber == 0
                ? connectionInfo.Secured ? 993 : 143
                : connectionInfo.PortNumber;
            var client = new ImapClient();
            client.Connect(connectionInfo.Server, portNumber, connectionInfo.Secured);
            client.Authenticate(connectionInfo.Login, connectionInfo.Password);
            return client;
        }
    }
}