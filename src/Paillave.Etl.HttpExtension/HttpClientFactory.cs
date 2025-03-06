using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;

using System.Net.Http;


namespace Paillave.Etl.HttpExtension;
{
    public static class HttpClientFactory
    {
        public static HttpClient CreateHttpClient(IHttpConnectionInfo connectionParameters)
        {
            HttpClient client = new HttpClient();

            if (connectionParameters.ConnexionType == "None") { }
            else if (connectionParameters.ConnexionType == "Bearer")
            {
                if (connectionParameters.HeaderParts.Count > 0)
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", connectionParameters.HeaderParts[0]);
                }
            }
            else if (connectionParameters.ConnexionType == "Basic")
            {
                if (connectionParameters.HeaderParts.Count >= 2)
                {
                    string credentials = $"{connectionParameters.HeaderParts[0]}:{connectionParameters.HeaderParts[1]}";
                    string base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", base64Credentials);
                }
            }
            else if (connectionParameters.ConnexionType == "ApiKey")
            {
                if (connectionParameters.HeaderParts.Count > 0)
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", connectionParameters.HeaderParts[0]);
                }
            }
            else if (connectionParameters.ConnexionType == "CustomHeader")
            {
                if (connectionParameters.HeaderParts.Count >= 2)
                {
                    client.DefaultRequestHeaders.Add(connectionParameters.HeaderParts[0], connectionParameters.HeaderParts[1]);
                }
            }
            else
            {
                throw new NotImplementedException($"Authentication type '{connectionParameters.ConnexionType}' is not implemented.");
            }

            return client;
        }
    }
}
