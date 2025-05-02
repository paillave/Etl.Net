using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Paillave.Etl.Http;

public static class IHttpConnectionInfoEx
{
    public static HttpClient CreateHttpClient(
        IHttpConnectionInfo httpConnectionInfo,
        IHttpAdapterParameters adapterParameters
    )
    {
        HttpClient client = new HttpClient();
        client = ManageAdditionalHeaders(client, adapterParameters.AdditionalHeaders);
        client = ManageAuthentication(client, httpConnectionInfo, adapterParameters);
        return client;
    }

    private static HttpClient ManageAdditionalHeaders(
        HttpClient client,
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
        IHttpConnectionInfo connectionParameters,
        IHttpAdapterParameters adapterParameters
    )
    {
        if (connectionParameters.Authentication?.Bearer != null)
            connectionParameters.Authentication.Bearer.AddAuthenticationHeaders(client);
        else if (connectionParameters.Authentication?.Basic != null)
            connectionParameters.Authentication.Basic.AddAuthenticationHeaders(client);
        else if (connectionParameters.Authentication?.Digest != null)
            connectionParameters.Authentication.Digest.AddAuthenticationHeaders(client);
        else if (connectionParameters.Authentication?.Digest != null)
            connectionParameters.Authentication.Digest.AddAuthenticationHeaders(client);
        else if (connectionParameters.Authentication?.Xcbaccess != null)
        {
            connectionParameters.Authentication.Xcbaccess.SetMethodPathBody(
                adapterParameters.Method,
                connectionParameters.Url,
                HttpHelpers.GetRequestBodyAsString(
                    adapterParameters.Body,
                    adapterParameters.RequestFormat
                )
            );
            connectionParameters.Authentication.Xcbaccess.AddAuthenticationHeaders(client);
        }

        return client;
    }
}
