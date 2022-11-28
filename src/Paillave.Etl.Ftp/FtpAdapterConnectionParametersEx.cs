using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using FluentFTP;

namespace Paillave.Etl.Ftp
{
    internal static class FtpAdapterConnectionParametersEx
    {
        private static Dictionary<string, string> GetDictionary(string values)
        {
            if (string.IsNullOrWhiteSpace(values)) return new Dictionary<string, string>();
            return values
                .Split(',', StringSplitOptions.None)
                .Select(i => i.Split('='))
                .ToDictionary(i => i[0], i => i[1]);
        }
        public static FtpClient CreateFtpClient(this IFtpConnectionInfo connectionParameters)
        {
            FtpClient client = new FtpClient(connectionParameters.Server, connectionParameters.Login, connectionParameters.Password, connectionParameters.PortNumber);
            var certificateChecks = new List<Func<X509Certificate, bool>>();
            var makeChecks = !(connectionParameters.NoCheck ?? false);
            if (makeChecks)
            {
                if (!string.IsNullOrWhiteSpace(connectionParameters.FingerPrintSha1))
                    certificateChecks.Add(i => string.Equals(i.GetCertHashString(), connectionParameters.FingerPrintSha1.Replace(":", ""), StringComparison.InvariantCultureIgnoreCase));
                if (!string.IsNullOrWhiteSpace(connectionParameters.SerialNumber))
                    certificateChecks.Add(i => string.Equals(i.GetSerialNumberString(), connectionParameters.SerialNumber.Replace(":", ""), StringComparison.InvariantCultureIgnoreCase));
                if (!string.IsNullOrWhiteSpace(connectionParameters.PublicKey))
                    certificateChecks.Add(i => string.Equals(i.GetPublicKeyString(), connectionParameters.PublicKey.Replace(":", ""), StringComparison.InvariantCultureIgnoreCase));
                if (connectionParameters.IssuerChecks != null && connectionParameters.IssuerChecks.Count > 0)
                    certificateChecks.Add(i =>
                        connectionParameters.IssuerChecks.GroupJoin(
                            GetDictionary(i.Issuer),
                            l => l.Key,
                            r => r.Key,
                            (l, r) => string.Equals(l.Value, r.Select(i => i.Value).FirstOrDefault(), StringComparison.InvariantCultureIgnoreCase)
                        , StringComparer.InvariantCultureIgnoreCase).All(i => i));
                if (connectionParameters.SubjectChecks != null && connectionParameters.SubjectChecks.Count > 0)
                    certificateChecks.Add(i =>
                        connectionParameters.SubjectChecks.GroupJoin(
                            GetDictionary(i.Subject),
                            l => l.Key,
                            r => r.Key,
                            (l, r) => string.Equals(l.Value, r.Select(i => i.Value).FirstOrDefault(), StringComparison.InvariantCultureIgnoreCase)
                        , StringComparer.InvariantCultureIgnoreCase).All(i => i));
            }
            if (certificateChecks.Count > 0)
            {
                client.Config.ValidateAnyCertificate = !makeChecks;

                if (connectionParameters.Tls ?? false)
                    client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
                else if (connectionParameters.Ssl ?? false)
                    client.Config.EncryptionMode = FtpEncryptionMode.Implicit;

                client.ValidateCertificate += (c, e) => e.Accept = certificateChecks.All(certificateCheck => certificateCheck(e.Certificate));
            }
            client.Connect();
            return client;
        }
    }
}