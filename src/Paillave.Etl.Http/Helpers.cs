using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Paillave.Etl.Http
{
    public static class Helpers
    {
        // Updated to handle "img" for all image formats
        private static HttpContent GetRequestBodyAsHttpContent(object? body, string requestFormat)
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
                    return new StringContent(body?.ToString() ?? "", Encoding.UTF8, "text/plain");

                case "html":
                    return new StringContent(body?.ToString() ?? "", Encoding.UTF8, "text/html");

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

        public static string GetRequestBodyAsString(object? body, string requestFormat)
        {
            var httpContent = GetRequestBodyAsHttpContent(body, requestFormat);

            if (requestFormat.ToLower().StartsWith("img"))
            {
                var byteArray = httpContent.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                return Convert.ToBase64String(byteArray);
            }

            return httpContent.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public static Task<HttpResponseMessage> GetResponse(
            IHttpConnectionInfo connectionParameters,
            HttpAdapterParametersBase adapterParametersBase,
            HttpClient httpClient,
            StreamContent? stream = null
        )
        {
            var uriBuilder = new UriBuilder(connectionParameters.Url);

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
                Method = new HttpMethod(adapterParametersBase.Method.ToString()),
            };

            if (adapterParametersBase.Method == HttpMethods.POST)
            {
                // requestMessage.Content =
                //     stream
                //     ?? Helpers.GetRequestBody(
                //         adapterParametersBase.Body,
                //         adapterParametersBase.RequestFormat
                //     );
                requestMessage.Content = Helpers.GetRequestBodyAsHttpContent(
                    stream?.GetJsonBody() ?? adapterParametersBase.Body,
                    adapterParametersBase.RequestFormat
                );
            }

            return adapterParametersBase.Method switch
            {
                HttpMethods.GET => httpClient.GetAsync(finalUri),
                HttpMethods.POST => httpClient.SendAsync(requestMessage),
                // "PUT" => httpClient.SendAsync(requestMessage),
                // "DELETE" => httpClient.DeleteAsync(finalUri),
                // "PATCH" => httpClient.SendAsync(requestMessage),
                // "HEAD" => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, finalUri)),
                // "OPTIONS" => httpClient.SendAsync(
                //     new HttpRequestMessage(HttpMethod.Options, finalUri)
                // ),
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
            string nc = "00000001";
            string cnonce = "xyz"; // Client nonce (a random string)

            // Compute HA1 and HA2 hashes, then response hash
            string ha1 = ComputeMd5Hash($"{username}:{realm}:{password}");
            string ha2 = ComputeMd5Hash($"{method}:{uri}");

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

        public static string Sign(
            string timestamp,
            string method,
            string path,
            string body,
            string signingKey
        )
        {
            try
            {
                string message = $"{timestamp}{method}{path}{body}";

                byte[] hmacKey;
                try
                {
                    hmacKey = Convert.FromBase64String(signingKey);
                }
                catch (FormatException)
                {
                    hmacKey = Encoding.UTF8.GetBytes(signingKey);
                }

                using var hmac = new HMACSHA256(hmacKey);
                byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return Convert.ToBase64String(signature);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to generate signature", e);
            }
        }
    }
}
