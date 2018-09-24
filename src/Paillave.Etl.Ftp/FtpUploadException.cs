using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Paillave.Etl.Ftp
{
    public class FtpUploadException:Exception
    {
        public FtpUploadException(FtpStatusCode statusCode, string statusDescription) : base($"failed to upload. {statusCode}: {statusDescription}")
        {

        }
    }
}
