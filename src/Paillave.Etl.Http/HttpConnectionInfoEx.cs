using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Paillave.Etl.Http;

public static class IHttpConnectionInfoEx
{
    public static HttpClient CreateHttpClient(
        this IHttpConnectionInfo httpConnectionInfo,
        Dictionary<string, string>? extraHeaders
    )
    {
        HttpClient client = new HttpClient();
        client = ManageAdditionalHeaders(client, httpConnectionInfo, extraHeaders);
        client = ManageAuthentication(client, httpConnectionInfo);
        return client;
    }

    private static HttpClient ManageAdditionalHeaders(
        HttpClient client,
        IHttpConnectionInfo connectionParameters,
        Dictionary<string, string>? extraHeaders
    )
    {
        if (extraHeaders != null)
        {
            foreach (var header in extraHeaders)
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
        IHttpConnectionInfo connectionParameters
    )
    {
        if (connectionParameters.Authentication?.Bearer != null)
            connectionParameters.Authentication.Bearer.AddAuthenticationHeaders(client);
        else if (connectionParameters.Authentication?.Basic != null)
            connectionParameters.Authentication.Basic.AddAuthenticationHeaders(client);
        else if (connectionParameters.Authentication?.Digest != null)
            connectionParameters.Authentication.Digest.AddAuthenticationHeaders(client);

        return client;
    }
}
