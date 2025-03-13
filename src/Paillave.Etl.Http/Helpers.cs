using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Paillave.Etl.Http
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
                            return new StringContent(
                                stringWriter.ToString(),
                                Encoding.UTF8,
                                "application/xml"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            "Failed to serialize object to XML.",
                            ex
                        );
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
            HttpClient httpClient,
            StreamContent? stream = null
        )
        {
            // Base URL and slug
            var baseUrl = connectionParameters.Url?.TrimEnd('/');
            var slug = adapterParametersBase.Slug?.TrimStart('/');
            var combinedUrl = $"{baseUrl}/{slug}".TrimEnd('/');
            var uriBuilder = new UriBuilder(combinedUrl);

            // Handle query parameters
            if (
                adapterParametersBase.AdditionalParameters != null
                && adapterParametersBase.AdditionalParameters.Any()
            )
            {
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

                foreach (var param in adapterParametersBase.AdditionalParameters)
                {
                    query[param.Key] = param.Value; // Avoid duplicates, update if key exists
                }

                uriBuilder.Query = query.ToString();
            }

            var finalUri = uriBuilder.ToString();

            var requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(finalUri),
                Method = new HttpMethod(adapterParametersBase.Method.ToUpper()),
            };

            if (adapterParametersBase.Method.ToUpper() is "POST" or "PUT" or "PATCH")
            {
                requestMessage.Content =
                    stream
                    ?? Helpers.GetRequestBody(
                        adapterParametersBase.Body,
                        adapterParametersBase.RequestFormat
                    );
            }

            return adapterParametersBase.Method.ToUpper() switch
            {
                "GET" => httpClient.GetAsync(finalUri),
                "POST" => httpClient.SendAsync(requestMessage),
                "PUT" => httpClient.SendAsync(requestMessage),
                "DELETE" => httpClient.DeleteAsync(finalUri),
                "PATCH" => httpClient.SendAsync(requestMessage),
                "HEAD" => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, finalUri)),
                "OPTIONS" => httpClient.SendAsync(
                    new HttpRequestMessage(HttpMethod.Options, finalUri)
                ),
                _ => throw new NotImplementedException(
                    $"HTTP method '{adapterParametersBase.Method}' is not implemented."
                ),
            };
        }

        public static string ExtractHeaderValue(string header, string key)
        {
            var start = header.IndexOf(key + "=") + key.Length + 2; // Skip "key=" and the quote
            var end = header.IndexOf('"', start);
            return header.Substring(start, end - start);
        }

        public static string GenerateDigestAuthHeader(
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

        public static string ComputeMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
