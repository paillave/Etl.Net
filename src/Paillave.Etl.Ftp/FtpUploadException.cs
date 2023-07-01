using System;
using System.Net;

namespace Paillave.Etl.Ftp
{
    public class FtpUploadException:Exception
    {
        public FtpUploadException(FtpStatusCode statusCode, string statusDescription) : base($"failed to upload. {statusCode}: {statusDescription}")
        {

        }
    }
}
