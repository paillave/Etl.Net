using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Paillave.Etl.Http;

public class HttpAuthentication
{
    public BearerAuthentication? Bearer { get; set; }
    public BasicAuthentication? Basic { get; set; }
    public DigestAuthentication? Digest { get; set; }
    public XCBACCESSAuthentication? Xcbaccess { get; set; }
}

public abstract class AbstractAuthentication
{
    public abstract HttpClient AddAuthenticationHeaders(HttpClient client);
}

public class DigestAuthentication : AbstractAuthentication
{
    public required string Url { get; set; }
    public required string User { get; set; }
    public required string Password { get; set; }
    public required string QualityOfProtection { get; set; }
    public required DigestAlgorithm Algorithm { get; set; } = DigestAlgorithm.MD5;

    public override HttpClient AddAuthenticationHeaders(HttpClient client)
    {
        throw new NotImplementedException("Digest authentication not tested enough");

        var response = client.GetAsync(Url).Result;

        // If response is 401, extract necessary values (like nonce) from the headers
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var authHeader = response.Headers.WwwAuthenticate.ToString();
            string realm = HttpHelpers.ExtractHeaderValue(authHeader, "realm");
            string nonce = HttpHelpers.ExtractHeaderValue(authHeader, "nonce");
            string opaque = HttpHelpers.ExtractHeaderValue(authHeader, "opaque");

            // Step 2: Generate the Digest Authentication header
            var digestAuthHeader = HttpHelpers.GenerateDigestAuthHeader(
                User,
                Password,
                realm,
                nonce,
                QualityOfProtection,
                Url,
                opaque
            );

            // Step 3: Send the request again with the Digest Authentication header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Digest",
                digestAuthHeader
            );
        }

        return client;
    }
}

public enum DigestAlgorithm
{
    MD5 = 0,
}

public class BasicAuthentication : AbstractAuthentication
{
    public required string User { get; set; }
    public required string Password { get; set; }

    public override HttpClient AddAuthenticationHeaders(HttpClient client)
    {
        string credentials = $"{User}:{Password}";
        string base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            base64Credentials
        );
        return client;
    }
}

public class BearerAuthentication : AbstractAuthentication
{
    public required string Authority { get; set; }
    public required string ClientId { get; set; }
    public required string Passphrase { get; set; }

    public override HttpClient AddAuthenticationHeaders(HttpClient client)
    {
        throw new NotImplementedException("Bearer authentication not implemented");
        // return client;
    }
}

public class XCBACCESSAuthentication : AbstractAuthentication
{
    public required string AccessKey { get; set; }
    public required string SigningKey { get; set; }
    public required string Passphrase { get; set; }

    public HttpMethodCustomEnum? Method { get; set; }
    public string? Path { get; set; }
    public string? Body { get; set; }

    public void SetMethodPathBody(HttpMethodCustomEnum method, string path, string? body)
    {
        Method = method;
        Path = path;
        Body = body;
    }

    public override HttpClient AddAuthenticationHeaders(HttpClient client)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = HttpHelpers.Sign(
            timestamp,
            Method?.ToString()
                ?? throw new ArgumentNullException(
                    "Method must be set for XCBACCESS Authentication"
                ),
            Path
                ?? throw new ArgumentNullException("Path must be set for XCBACCESS Authentication"),
            Body,
            SigningKey
        );

        client.DefaultRequestHeaders.Add("X-CB-ACCESS-KEY", AccessKey);
        client.DefaultRequestHeaders.Add("X-CB-ACCESS-SIGNATURE", signature);
        client.DefaultRequestHeaders.Add("X-CB-ACCESS-TIMESTAMP", timestamp);
        client.DefaultRequestHeaders.Add("X-CB-ACCESS-PASSPHRASE", Passphrase);

        return client;
    }
}
