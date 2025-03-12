using System.Text;
using Newtonsoft.Json;
using Paillave.Etl.Core;

namespace Paillave.Etl.JsonFile;

public static class Helpers
{
    public static T? DeserializeJsonToObject<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonConvert.DeserializeObject<T>(json);
    }

    public static StringContent GetStringContentFromObject(object? body, Encoding? encoding = null)
    {
        var jsonBody = JsonConvert.SerializeObject(body);
        encoding ??= Encoding.UTF8;
        return new StringContent(jsonBody, encoding, "application/json");
    }

    public static T? GetFileValueFromJson<T, TIntermediary>(
        string json,
        Func<TIntermediary, T> mapper
    )
        where T : class, IFileValue
    {
        var intermediaryObj = DeserializeJsonToObject<TIntermediary>(json);

        return intermediaryObj == null ? default : mapper(intermediaryObj);
    }

    public static T? GetFileValueFromObject<T, TIntermediary>(
        TIntermediary intermediaryObj,
        Func<TIntermediary, T> mapper
    )
        where T : class, IFileValue
    {
        if (intermediaryObj == null)
            return default;

        return mapper(intermediaryObj);
    }

    public static T? GetObjectFromFileValue<T>(IFileValue fileValue, Encoding? encoding = null)
    {
        if (fileValue == null || fileValue.Content == null)
            return default;

        byte[]? bytes = fileValue.Content switch
        {
            byte[] b => b,
            Stream s => 
            {
                if (s.CanSeek)
                    s.Position = 0;

                using var ms = new MemoryStream();
                s.CopyTo(ms);
                return ms.ToArray();
            },
            _ => null
        };

        if (bytes == null)
            return default;

        encoding ??= Encoding.UTF8;

        string json = encoding.GetString(bytes);

        return DeserializeJsonToObject<T>(json);
    }

    public static T? CreateFileValueFromObject<T>(
        object obj,
        Func<byte[], T> fileValueFactory,
        Encoding? encoding = null
    )
        where T : class, IFileValue
    {
        var json = JsonConvert.SerializeObject(obj);
        encoding ??= Encoding.UTF8;
        var bytes = encoding.GetBytes(json);
        return fileValueFactory(bytes);
    }
}
