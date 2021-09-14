using System;
using System.Collections.Generic;
using System.IO;
// using Renci.SshNet;

namespace Paillave.Etl.Dropbox
{

    public class DropboxConnectionInfo : IDropboxConnectionInfo
    {
        public string Token { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int MaxAttempts { get; set; }
    }
}
