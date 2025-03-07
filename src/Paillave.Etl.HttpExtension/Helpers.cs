using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Serialization;
using Paillave.Etl.Core;

namespace Paillave.Etl.HttpExtension
{
    public static class Helpers
    {
        // Updated to handle "img" for all image formats
        private static HttpContent GetRequestBody(object? body, string requestFormat)
        {
            if (body == null)
            {
                return requestFormat.ToLower() switch
                {
                    "json" => new StringContent("{}", Encoding.UTF8, "application/json"),
                    "xml" => new StringContent("<root></root>", Encoding.UTF8, "application/xml"),
                    "text" => new StringContent(string.Empty, Encoding.UTF8, "text/plain"),
                    "html" => new StringContent("<html></html>", Encoding.UTF8, "text/html"),
                    "img" => new ByteArrayContent(Array.Empty<byte>()), 
                    _ => throw new NotImplementedException(),
                };
            }

            // Process based on response format
            switch (requestFormat.ToLower())
            {
                case "json":
                    var jsonBody = JsonConvert.SerializeObject(body);
                    return new StringContent(jsonBody, Encoding.UTF8, "application/json");

                case "xml":
                    try
                    {
                        var xmlSerializer = new XmlSerializer(body.GetType());
                        using (var stringWriter = new StringWriter())
                        {
                            xmlSerializer.Serialize(stringWriter, body);
                            return new StringContent(stringWriter.ToString(), Encoding.UTF8, "application/xml");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Failed to serialize object to XML.", ex);
                    }

                case "text":
                    return new StringContent(body.ToString(), Encoding.UTF8, "text/plain");

                case "html":
                    return new StringContent(body.ToString(), Encoding.UTF8, "text/html");

                case "img":
                    if (body is byte[] byteArray)
                    {
                        return new ByteArrayContent(byteArray);
                    }
                    throw new InvalidOperationException("Expected byte array for image data.");

                default:
                    throw new NotImplementedException();
            }
        }

        public static Task<HttpResponseMessage> GetResponse(
            HttpAdapterConnectionParameters connectionParameters,
            HttpAdapterParametersBase parametersBase,
            HttpClient httpClient
        )
        {
            var url = new Uri(
                new Uri(connectionParameters.Url.TrimEnd('/')),
                parametersBase.Slug
            ).ToString();

            return parametersBase.Method.ToUpper() switch
            {
                "GET" => httpClient.GetAsync(url),
                "POST" => httpClient.PostAsync(
                    url,
                    Helpers.GetRequestBody(parametersBase.Body, parametersBase.RequestFormat)
                ),
                "PUT" => httpClient.PutAsync(
                    url,
                    Helpers.GetRequestBody(parametersBase.Body, parametersBase.RequestFormat)
                ),
                "DELETE" => httpClient.DeleteAsync(url),
                "PATCH" => httpClient.PatchAsync(
                    url,
                    Helpers.GetRequestBody(parametersBase.Body, parametersBase.RequestFormat)
                ),
                "HEAD" => httpClient.HeadAsync(url),
                "OPTIONS" => httpClient.OptionsAsync(url),
                _ => throw new NotImplementedException($"HTTP method '{parametersBase.Method}' is not implemented."),
            };
        }
    }
}
