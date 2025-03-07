using System.Net.Http;
using System.Text;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Newtonsoft.Json;

namespace Paillave.Etl.HttpExtension;

public static class Helpers
{
    public static StringContent GetJsonBody(this object? body)
    {
        var jsonBody = JsonConvert.SerializeObject(body);
        return new StringContent(jsonBody, Encoding.UTF8, "application/json");
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
