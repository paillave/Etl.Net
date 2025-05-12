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
    public static class HttpHelpers
    {
        // Updated to handle "img" for all image formats
        private static HttpContent GetRequestBodyAsHttpContent(
            object? body,
            RequestFormat requestFormat
        )
        {
            if (body == null)
            {
                switch (requestFormat)
                {
                    case RequestFormat.Json:
                        return new StringContent("{}", Encoding.UTF8, "application/json");

                    case RequestFormat.Xml:
                        return new StringContent("<root></root>", Encoding.UTF8, "application/xml");

                    case RequestFormat.PlainTxt:
                        return new StringContent(string.Empty, Encoding.UTF8, "text/plain");

                    case RequestFormat.Html:
                        return new StringContent("<html></html>", Encoding.UTF8, "text/html");

                    case RequestFormat.Jpeg:
                    case RequestFormat.Png:
                    case RequestFormat.Gif:
                    case RequestFormat.Svg:
                    case RequestFormat.WebP:
                        return new ByteArrayContent(Array.Empty<byte>());

                    default:
                        throw new NotImplementedException();
                }
            }

            // Process based on response format
            switch (requestFormat)
            {
                case RequestFormat.Json:
                    var jsonBody = JsonConvert.SerializeObject(body);
                    return new StringContent(jsonBody, Encoding.UTF8, "application/json");

                case RequestFormat.Xml:
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

                case RequestFormat.PlainTxt:
                    return new StringContent(body?.ToString() ?? "", Encoding.UTF8, "text/plain");

                case RequestFormat.Html:
                    return new StringContent(body?.ToString() ?? "", Encoding.UTF8, "text/html");

                case RequestFormat.Jpeg:
                case RequestFormat.Png:
                case RequestFormat.Gif:
                case RequestFormat.Svg:
                case RequestFormat.WebP:
                    if (body is byte[] byteArray)
                    {
                        return new ByteArrayContent(byteArray);
                    }
                    throw new InvalidOperationException("Expected byte array for image data.");

                default:
                    throw new NotImplementedException();
            }
        }

        public static string? GetRequestBodyAsString(object? body, RequestFormat requestFormat)
        {
            if (body == null)
                return null;

            var httpContent = GetRequestBodyAsHttpContent(body, requestFormat);

            if (
                new RequestFormat[]
                {
                    RequestFormat.Jpeg,
                    RequestFormat.Png,
                    RequestFormat.Gif,
                    RequestFormat.Svg,
                    RequestFormat.WebP,
                }.Contains(requestFormat)
            )
            {
                var byteArray = httpContent.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                return Convert.ToBase64String(byteArray);
            }

            return httpContent.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public static Task<HttpResponseMessage> GetResponse(
            IHttpConnectionInfo connectionParameters,
            IHttpAdapterParameters adapterParametersBase,
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

            if (adapterParametersBase.Method == HttpMethodCustomEnum.Post)
            {
                // requestMessage.Content =
                //     stream
                //     ?? Helpers.GetRequestBody(
                //         adapterParametersBase.Body,
                //         adapterParametersBase.RequestFormat
                //     );
                requestMessage.Content = GetRequestBodyAsHttpContent(
                    stream?.GetJsonBody() ?? adapterParametersBase.Body,
                    adapterParametersBase.RequestFormat
                );
            }

            return adapterParametersBase.Method switch
            {
                HttpMethodCustomEnum.Get => httpClient.GetAsync(finalUri),
                HttpMethodCustomEnum.Post => httpClient.SendAsync(requestMessage),
                // "PUT" => httpClient.SendAsync(requestMessage),
                // "DELETE" => httpClient.DeleteAsync(finalUri),
                // "PATCH" => httpClient.SendAsync(requestMessage),
                // "HEAD" => httpClient.SendAsync(new HttpRequestMessage(HttpMethodCustomEnum.Head, finalUri)),
                // "OPTIONS" => httpClient.SendAsync(
                //     new HttpRequestMessage(HttpMethodCustomEnum.Options, finalUri)
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
            string uri,
            string? body,
            string signingKey
        )
        {
            var url = new Uri(uri);
            var path = url.AbsolutePath;
            var capitalizedMethod = method.ToUpperInvariant();

            try
            {
                string message = $"{timestamp}{capitalizedMethod}{path}{body}";

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
