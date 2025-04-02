using System;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Paillave.Etl.Http;

public static class HttpResponseMessageEx
{
    public static string GetFileName(this HttpResponseMessage? response, string url)
    {
        var filename =
            response?.Content.Headers.ContentDisposition?.FileName
            ?? url.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();

        var extension = Path.GetExtension(filename);
        return !string.IsNullOrEmpty(extension) ? filename : $"{filename}.json";
    }
}
