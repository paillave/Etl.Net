using System;
using System.Collections.Generic;
using System.Net.Http;
using Fluid;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public static class IHttpConnectionInfoEx
{
    public static HttpClient CreateHttpClient(
        IHttpConnectionInfo httpConnectionInfo,
        IHttpAdapterParameters adapterParameters,
        IFileValueMetadata? additionalMetadata=null
    )
    {
        HttpClient client = new HttpClient();
        client = ManageHeaders(client, httpConnectionInfo.HttpHeaders);
        client = ManageAdditionalParameters(client, httpConnectionInfo, adapterParameters);
        client = ManageAuthentication(client, httpConnectionInfo, adapterParameters);
        return client;
    }

    private static HttpClient ManageHeaders(HttpClient client, Dictionary<string, string>? headers)
    {
        if (headers != null)
        {
            foreach (var header in headers)
            {
                if (client.DefaultRequestHeaders.Contains(header.Key))
                    throw new ArgumentException(
                        $"Header '{header.Key}' already exists in the request."
                    );

                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return client;
    }

    private static HttpClient ManageAdditionalParameters(
        HttpClient client,
        IHttpConnectionInfo connectionParameters,
        object? additionalMetadata=null)
    {
        connectionParameters.Url = ApplyFluidParameters(connectionParameters.Url, additionalMetadata);
        return client;
    }

    private static string ApplyFluidParameters(string text, object? additionalMetadata=null)
    {
        if (additionalMetadata == null) return text;
        var parser = new FluidParser();
        if (parser.TryParse(text, out var fTemplate, out var error))
        {
            var context = new TemplateContext(additionalMetadata ?? new(), new TemplateOptions
            {
                ModelNamesComparer = StringComparer.InvariantCultureIgnoreCase,
                MemberAccessStrategy = new UnsafeMemberAccessStrategy()
            }, true);
            return fTemplate.Render(context);
        }
        else
        {
            throw new Exception($"Error: {error}");
        }
    }

    private static HttpClient ManageAuthentication(
        HttpClient client,
        IHttpConnectionInfo connectionParameters,
        IHttpAdapterParameters adapterParameters,
        object? additionalMetadata=null
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
            string? body = HttpHelpers.GetRequestBodyAsString(
                    adapterParameters.Body,
                    adapterParameters.RequestFormat
                );
            if (body != null && additionalMetadata != null)
            {
                body=ApplyFluidParameters(body,additionalMetadata);
            }

            connectionParameters.Authentication.Xcbaccess.SetMethodPathBody(
                adapterParameters.Method,
                connectionParameters.Url,
                body
            );
            connectionParameters.Authentication.Xcbaccess.AddAuthenticationHeaders(client);
        }

        return client;
    }
}
