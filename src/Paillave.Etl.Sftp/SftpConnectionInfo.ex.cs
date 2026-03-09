using System.Collections.Generic;
using Renci.SshNet;
using Paillave.Etl.Core;

namespace Paillave.Etl.Sftp;

internal static class SftpConnectionInfoEx
{
    public static ConnectionInfo CreateConnectionInfo(this ISftpConnectionInfo info)
    {
        var authenticationMethods = new List<AuthenticationMethod>();
        if (!string.IsNullOrWhiteSpace(info.Password))
            authenticationMethods.Add(new PasswordAuthenticationMethod(info.Login, info.Password));
        if (!string.IsNullOrWhiteSpace(info.PrivateKey))
        {
            var privateKeyStream = info.PrivateKey.ToStream();
            if (string.IsNullOrWhiteSpace(info.PrivateKeyPassPhrase))
                authenticationMethods.Add(new PrivateKeyAuthenticationMethod(info.Login, new PrivateKeyFile(privateKeyStream)));
            else
                authenticationMethods.Add(new PrivateKeyAuthenticationMethod(info.Login, new PrivateKeyFile(privateKeyStream, info.PrivateKeyPassPhrase)));
        }
        return new ConnectionInfo(
            info.Server,
            info.PortNumber,
            info.Login,
            authenticationMethods.ToArray()
        );
    }
}