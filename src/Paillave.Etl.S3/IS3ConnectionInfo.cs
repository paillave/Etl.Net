namespace Paillave.Etl.S3;
public interface IS3ConnectionInfo
{
    string RootFolder { get; set; }
    string ServiceUrl { get; set; }
    int? PortNumber { get; set; }
    int MaxAttempts { get; set; }
    string Bucket { get; set; }
    string AccessKeyId { get; set; }
    string AccessKeySecret { get; set; }
}
