using Amazon.Runtime;
using Amazon.S3;

namespace Paillave.Etl.S3;
internal static class S3ConnectionInfoEx
{
    public static S3Bucket CreateBucketConnection(this IS3ConnectionInfo info)
        => new(
            new BasicAWSCredentials(info.AccessKeyId, info.AccessKeySecret),
            new AmazonS3Config
            {
                ServiceURL = info.PortNumber == null ? info.ServiceUrl : $"{info.ServiceUrl}:{info.PortNumber}",
                UseHttp = false,
                ForcePathStyle = true
            },
            info.Bucket);
}
