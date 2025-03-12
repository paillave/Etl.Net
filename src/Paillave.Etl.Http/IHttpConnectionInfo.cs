using System;
using System.Collections.Generic;

namespace Paillave.Etl.Http;

public interface IHttpConnectionInfo
{
    public Uri Url { get; set; }
    // public List<string>? AuthParameters { get; set; }
    public HttpAuthentication? Authentication { get; set; }
    // public string? AuthenticationType { get; set; }
    public int MaxAttempts { get; set; }
    public Dictionary<string, string>? HttpHeaders { get; set; }
}
public class HttpAuthentication
{
    public BearerAuthentication? Bearer { get; set; }
    public BasicAuthentication? Basic { get; set; }
}
public class DigestAuthentication
{
    public required string User { get; set; }
    public required string Password { get; set; }
    public required string QualityOfProtection { get; set; }
    public required DigestAlgorithm Algorithm { get; set; } = DigestAlgorithm.MD5;
}
public enum DigestAlgorithm
{
    MD5 = 0
}
public class BasicAuthentication
{
    public required string User { get; set; }
    public required string Password { get; set; }
}
public class BearerAuthentication
{
    public required string Authority { get; set; }
    public required string ClientId { get; set; }
    public required string Passphrase { get; set; }
}