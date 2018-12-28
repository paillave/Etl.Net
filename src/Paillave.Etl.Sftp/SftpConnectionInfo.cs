using System;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet;

namespace Paillave.Etl.Sftp
{
    public class SftpConnectionInfo
    {
        public string Server { get; set; }
        public int PortNumber { get; set; } = 22;
        public string Login { get; set; }
        public string Password { get; set; }
        public string PathToPrivateKey { get; set; }
        public string PrivateKeyPassPhrase { get; set; }
        public Stream PrivateKeyStream { get; set; }
        internal ConnectionInfo CreateConnectionInfo()
        {
            var authenticationMethods = new List<AuthenticationMethod>();
            if (!string.IsNullOrWhiteSpace(Password))
                authenticationMethods.Add(new PasswordAuthenticationMethod(this.Login, this.Password));
            if (!string.IsNullOrWhiteSpace(PathToPrivateKey))
            {
                if (string.IsNullOrWhiteSpace(this.PrivateKeyPassPhrase))
                    authenticationMethods.Add(new PrivateKeyAuthenticationMethod(this.Login, new PrivateKeyFile(this.PathToPrivateKey)));
                else
                    authenticationMethods.Add(new PrivateKeyAuthenticationMethod(this.Login, new PrivateKeyFile(this.PathToPrivateKey, this.PrivateKeyPassPhrase)));
            }
            if (PrivateKeyStream != null)
            {
                if (string.IsNullOrWhiteSpace(this.PrivateKeyPassPhrase))
                    authenticationMethods.Add(new PrivateKeyAuthenticationMethod(this.Login, new PrivateKeyFile(this.PrivateKeyStream)));
                else
                    authenticationMethods.Add(new PrivateKeyAuthenticationMethod(this.Login, new PrivateKeyFile(this.PrivateKeyStream, this.PrivateKeyPassPhrase)));
            }
            return new ConnectionInfo(
                this.Server,
                this.PortNumber,
                this.Login,
                authenticationMethods.ToArray()
            );
        }
    }
}
