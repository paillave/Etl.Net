using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Paillave.Etl.HttpExtension;

public static class HttpClientFactory
{
    public static HttpClient CreateHttpClient(IHttpConnectionInfo connectionParameters)
    {
        HttpClient client = new HttpClient();

        switch (connectionParameters.AuthenticationType)
        {
            case "None":
                // No authentication needed
                break;

            case "Bearer":
                if (connectionParameters.HeaderParts.Count == 1)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        connectionParameters.HeaderParts[0]
                    );
                }
                else
                    throw new ArgumentException(
                        "Wrong number of HeaderParts for Bearer authentication."
                    );
                break;

            case "Basic":
                if (connectionParameters.HeaderParts.Count == 2)
                {
                    string credentials =
                        $"{connectionParameters.HeaderParts[0]}:{connectionParameters.HeaderParts[1]}";
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
                        "Wrong number of HeaderParts for Basic authentication."
                    );
                break;

            case "ApiKey":
                if (connectionParameters.HeaderParts.Count == 1)
                {
                    client.DefaultRequestHeaders.Add(
                        "X-API-Key",
                        connectionParameters.HeaderParts[0]
                    );
                }
                else
                    throw new ArgumentException(
                        "Wrong number of HeaderParts for ApiKey authentication."
                    );
                break;

            case "CustomHeader":
                if (connectionParameters.HeaderParts.Count % 2 == 0)
                {
                    for (int i = 0; i < connectionParameters.HeaderParts.Count; i += 2)
                    {
                        // Assuming HeaderParts is a list with alternating key-value pairs
                        client.DefaultRequestHeaders.Add(
                            connectionParameters.HeaderParts[i],
                            connectionParameters.HeaderParts[i + 1]
                        );
                    }
                }
                else
                    throw new ArgumentException(
                        "Wrong number of HeaderParts for CustomHeader authentication."
                    );
                break;

            default:
                throw new NotImplementedException(
                    $"Authentication type '{connectionParameters.AuthenticationType}' is not implemented."
                );
        }

        return client;
    }
}
