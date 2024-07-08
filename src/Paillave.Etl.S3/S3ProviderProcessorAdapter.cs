using System.ComponentModel.DataAnnotations;
using System.Linq;
using Paillave.Etl.Core;
using System.Net;

namespace Paillave.Etl.S3;
public class S3AdapterConnectionParameters : IS3ConnectionInfo
{
    public string? RootFolder { get; set; }
    [Required]
    public string ServiceUrl { get; set; }
    public int? PortNumber { get; set; }
    public int MaxAttempts { get; set; } = 3;
    [Required]
    public string Bucket { get; set; }
    [Required]
    public string AccessKeyId { get; set; }
    [Required]
    public string AccessKeySecret { get; set; }
}
public class S3AdapterProviderParameters
{
    public string? SubFolder { get; set; }
    public string? FileNamePattern { get; set; }
    public bool? Recursive { get; set; }
}
public class S3AdapterProcessorParameters
{
    public string SubFolder { get; set; }
    public bool UseStreamCopy { get; set; } = true;
}
public class S3ProviderProcessorAdapter : ProviderProcessorAdapterBase<S3AdapterConnectionParameters, S3AdapterProviderParameters, S3AdapterProcessorParameters>
{
    public override string Description => "Get and save files on an S3 server";
    public override string Name => "S3";
    protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, S3AdapterConnectionParameters connectionParameters, S3AdapterProviderParameters inputParameters)
        => new S3FileValueProvider(code, name, connectionName, connectionParameters, inputParameters);
    protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, S3AdapterConnectionParameters connectionParameters, S3AdapterProcessorParameters outputParameters)
        => new S3FileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
}
