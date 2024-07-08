using System;
using System.Collections.Generic;
using System.IO;

namespace Paillave.Etl.S3;

public class S3ConnectionInfo : IS3ConnectionInfo
{
    public string RootFolder { get; set; }
    public string ServiceUrl { get; set; }
    public int? PortNumber { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public string Bucket { get; set; }
    public string AccessKeyId { get; set; }
    public string AccessKeySecret { get; set; }
}
