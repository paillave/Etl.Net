using System.Collections.Generic;
using Dropbox.Api;
// using Renci.SshNet;

namespace Paillave.Etl.Dropbox
{
    internal static class DropboxConnectionInfoEx
    {
        public static DropboxClient CreateConnectionInfo(this IDropboxConnectionInfo info)
        {
            if (!string.IsNullOrWhiteSpace(info.AppSecret))
            {
                return new DropboxClient(info.Token, info.AppKey, info.AppSecret);
            }
            else if (!string.IsNullOrWhiteSpace(info.AppKey))
            {
                return new DropboxClient(info.Token, info.AppKey);
            }
            else
            {
                return new DropboxClient(info.Token);
            }
            throw new System.Exception("dropbox connection information must be provided");
        }
    }
}