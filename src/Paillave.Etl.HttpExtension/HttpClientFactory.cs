using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.HttpExtension;

public static class HttpClientFactory
{
    public static HttpClient GetClient(
        IHttpConnectionInfo connectionParameters,
        HttpAdapterParametersBase adapterParametersBase
    )
    {
        HttpClient client = new HttpClient();
        client = ManageAdditionalHeaders(client, connectionParameters, adapterParametersBase);
        client = ManageAuthentication(client, connectionParameters, adapterParametersBase);
        return client;
    }

    private static HttpClient ManageAdditionalHeaders(
        HttpClient client,
        IHttpConnectionInfo connectionParameters,
        HttpAdapterParametersBase adapterParametersBase
    )
    {
        if (adapterParametersBase.AdditionalHeaders != null)
        {
            foreach (var header in adapterParametersBase.AdditionalHeaders)
            {
                if (client.DefaultRequestHeaders.Contains(header.Key))
                    throw new ArgumentException(
                        $"Header '{header.Key}' already exists in the request."
                    );

                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        return client;
    }

    private static HttpClient ManageAuthentication(
        HttpClient client,
        IHttpConnectionInfo connectionParameters,
        HttpAdapterParametersBase adapterParametersBase
    )
    {
        switch (connectionParameters.AuthenticationType)
        {
            case "None":
                break;

            case "Bearer":
                if (connectionParameters.AuthParameters.Count == 1)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        connectionParameters.AuthParameters[0]
                    );
                }
                else
                    throw new ArgumentException(
                        "Wrong number of AuthParameters for Bearer authentication."
                    );
                break;

            case "Basic":
                if (connectionParameters.AuthParameters.Count == 2)
                {
                    string credentials =
                        $"{connectionParameters.AuthParameters[0]}:{connectionParameters.AuthParameters[1]}";
                    string base64Credentials = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(credentials)
                    );
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Basic",
                        base64Credentials
                    );
                }
                else
                    throw new ArgumentException(
                        "Wrong number of AuthParameters for Basic authentication."
                    );
                break;

            case "ApiKey":
                if (connectionParameters.AuthParameters.Count == 1)
                {
                    client.DefaultRequestHeaders.Add(
                        "X-API-Key",
                        connectionParameters.AuthParameters[0]
                    );
                }
                else
                    throw new ArgumentException(
                        "Wrong number of AuthParameters for ApiKey authentication."
                    );
                break;

            case "CustomHeader":
                if (connectionParameters.AuthParameters.Count % 2 == 0)
                {
                    for (int i = 0; i < connectionParameters.AuthParameters.Count; i += 2)
                    {
                        // Assuming AuthParameters is a list with alternating key-value pairs
                        client.DefaultRequestHeaders.Add(
                            connectionParameters.AuthParameters[i],
                            connectionParameters.AuthParameters[i + 1]
                        );
                    }
                }
                else
                    throw new ArgumentException(
                        "Wrong number of AuthParameters for CustomHeader authentication."
                    );
                break;

            // FIXME: WIP, logic to be validated
            case "Digest":
                if (connectionParameters.AuthParameters.Count >= 2)
                {
                    var uri = new Uri(
                        new Uri(connectionParameters.Url.TrimEnd('/')),
                        adapterParametersBase.Slug
                    ).ToString();

                    string username = connectionParameters.AuthParameters[0];
                    string password = connectionParameters.AuthParameters[1];
                    string qop =
                        connectionParameters.AuthParameters.Count > 2
                            ? connectionParameters.AuthParameters[2]
                            : "auth"; // Default to "auth" if qop is not specified
                    string algorithm =
                        connectionParameters.AuthParameters.Count > 3
                            ? connectionParameters.AuthParameters[3]
                            : "MD5"; // Default to MD5

                    var response = client.GetAsync(uri).Result;

                    // If response is 401, extract necessary values (like nonce) from the headers
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var authHeader = response.Headers.WwwAuthenticate.ToString();
                        string realm = ExtractHeaderValue(authHeader, "realm");
                        string nonce = ExtractHeaderValue(authHeader, "nonce");
                        string opaque = ExtractHeaderValue(authHeader, "opaque");

                        // Step 2: Generate the Digest Authentication header
                        var digestAuthHeader = GenerateDigestAuthHeader(
                            username,
                            password,
                            realm,
                            nonce,
                            qop,
                            uri,
                            opaque
                        );

                        // Step 3: Send the request again with the Digest Authentication header
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                            "Digest",
                            digestAuthHeader
                        );

                        response = client.GetAsync(uri).Result;
                        Console.WriteLine($"Response Status Code: {(int)response.StatusCode}");
                    }
                }
                else
                {
                    throw new ArgumentException(
                        "Wrong number of AuthParameters for Digest authentication."
                    );
                }
                break;

            default:
                throw new NotImplementedException(
                    $"Authentication type '{connectionParameters.AuthenticationType}' is not implemented."
                );
        }

        return client;
    }

    private static string ExtractHeaderValue(string header, string key)
    {
        var start = header.IndexOf(key + "=") + key.Length + 2; // Skip "key=" and the quote
        var end = header.IndexOf('"', start);
        return header.Substring(start, end - start);
    }

    private static string GenerateDigestAuthHeader(
        string username,
        string password,
        string realm,
        string nonce,
        string qop,
        string uri,
        string opaque
    )
    {
        string method = "GET";
        string nc = "00000001"; // Nonce count (you can increase this in multiple requests)
        string cnonce = "xyz"; // Client nonce (a random string)

        // Compute HA1 and HA2 hashes
        string ha1 = ComputeMd5Hash($"{username}:{realm}:{password}");
        string ha2 = ComputeMd5Hash($"{method}:{uri}");

        // Compute response hash
        string response = ComputeMd5Hash($"{ha1}:{nonce}:{nc}:{cnonce}:{qop}:{ha2}");

        return $"username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{uri}\", "
            + $"response=\"{response}\", qop={qop}, nc={nc}, cnonce=\"{cnonce}\", opaque=\"{opaque}\"";
    }

    private static string ComputeMd5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
