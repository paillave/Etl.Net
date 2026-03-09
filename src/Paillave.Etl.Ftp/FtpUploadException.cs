using System;
using System.Net;

namespace Paillave.Etl.Ftp;

public class FtpUploadException(FtpStatusCode statusCode, string statusDescription) : Exception($"failed to upload. {statusCode}: {statusDescription}")
{
}
