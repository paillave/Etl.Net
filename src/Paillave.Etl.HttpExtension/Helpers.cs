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


namespace Paillave.Etl.HttpExtension;

public static class Helpers
{
    private static StringContent GetResponseBody(object? body, string responseFormat)
    {
        if (body == null)
        {
            return responseFormat.ToLower() switch
            {
                "json" => new StringContent("{}", Encoding.UTF8, "application/json"),
                "xml" => new StringContent("<root></root>", Encoding.UTF8, "application/xml"),
                "text" => new StringContent(string.Empty, Encoding.UTF8, "text/plain"),
                "html" => new StringContent("<html></html>", Encoding.UTF8, "text/html"),
                _ => throw new NotImplementedException(),
            };
        }

        // Process based on response format
        switch (responseFormat.ToLower())
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
                    // return new StringContent("<root></root>", Encoding.UTF8, "application/xml"); // Return empty XML if serialization fails
                }

            case "text":
                return new StringContent(body.ToString(), Encoding.UTF8, "text/plain");

            case "html":
                return new StringContent(body.ToString(), Encoding.UTF8, "text/html");

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

        return parametersBase.Method switch
        {
            "Get" => httpClient.GetAsync(url),
            "Post" => httpClient.PostAsync(
                url,
                Helpers.GetJsonBody(parametersBase.Body)
            ),
            _ => throw new NotImplementedException(),
        };
    }
}
