
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Paillave.Etl.Http;

public static class HttpRequestBodyEx
{
    public static StringContent GetJsonBody(this object? body)
    {
        var jsonBody = JsonConvert.SerializeObject(body);
        return new StringContent(jsonBody, Encoding.UTF8, "application/json");
    }
}
