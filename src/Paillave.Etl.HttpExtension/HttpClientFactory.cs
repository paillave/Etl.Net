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
                if (connectionParameters.HeaderParts.Count > 0)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        connectionParameters.HeaderParts[0]
                    );
                }
                break;

            case "Basic":
                if (connectionParameters.HeaderParts.Count >= 2)
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
                break;

            case "ApiKey":
                if (connectionParameters.HeaderParts.Count > 0)
                {
                    client.DefaultRequestHeaders.Add(
                        "X-API-Key",
                        connectionParameters.HeaderParts[0]
                    );
                }
                break;

            case "CustomHeader":
                if (connectionParameters.HeaderParts.Count >= 2)
                {
                    client.DefaultRequestHeaders.Add(
                        connectionParameters.HeaderParts[0],
                        connectionParameters.HeaderParts[1]
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
}
