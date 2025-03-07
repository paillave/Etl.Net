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
            HttpAdapterParametersBase adapterParametersBase,
            HttpClient httpClient
        )
        {
            var uri = new Uri(
                new Uri(connectionParameters.Url.TrimEnd('/')),
                adapterParametersBase.Slug
            ).ToString();

            var requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = new HttpMethod(adapterParametersBase.Method.ToUpper())
            };

            if (adapterParametersBase.Method.ToUpper() == "POST" || adapterParametersBase.Method.ToUpper() == "PUT" || adapterParametersBase.Method.ToUpper() == "PATCH")
            {
                requestMessage.Content = Helpers.GetRequestBody(adapterParametersBase.Body, adapterParametersBase.RequestFormat);
            }

            return adapterParametersBase.Method.ToUpper() switch
            {
                "GET" => httpClient.GetAsync(uri),
                "POST" => httpClient.SendAsync(requestMessage),
                "PUT" => httpClient.SendAsync(requestMessage),
                "DELETE" => httpClient.DeleteAsync(uri),
                "PATCH" => httpClient.SendAsync(requestMessage),
                "HEAD" => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri)),
                "OPTIONS" => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Options, uri)),
                _ => throw new NotImplementedException($"HTTP method '{adapterParametersBase.Method}' is not implemented."),
            };
        }
    }
}
