using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paillave.Etl.Core;

namespace Paillave.Etl.JsonFile;

public static class Helpers
{
    public static T JsonToObject<T>(JObject json)
    {
        if (json == null)
            return default;

        return json.ToObject<T>();
    }

    public static JObject ObjectToJson<T>(T obj, bool indented = false)
    {
        if (obj == null)
            return new JObject();

        var formatting = indented ? Formatting.Indented : Formatting.None;
        string jsonString = JsonConvert.SerializeObject(obj, formatting);
        return JObject.Parse(jsonString);
    }

    //     public static T? JsonToFileValue<T, TIntermediary>(string json, Func<TIntermediary, T> mapper)
    //         where T : class, IFileValue
    //     {
    //         var intermediaryObj = JsonToObject<TIntermediary>(json);

    //         return intermediaryObj == null ? default : mapper(intermediaryObj);
    //     }

    //     public static T? ObjectToFileValue<T, TIntermediary>(
    //         TIntermediary intermediaryObj,
    //         Func<TIntermediary, T> mapper
    //     )
    //         where T : class, IFileValue
    //     {
    //         if (intermediaryObj == null)
    //             return default;

    //         return mapper(intermediaryObj);
    //     }

    public static T? FileValueToObject<T>(IFileValue fileValue, Encoding? encoding = null)
    {
        if (fileValue == null)
            return default;

        var content = fileValue.GetContent();

        if (content == null)
            return default;

        if (!(content is Stream stream))
            throw new InvalidOperationException("Content must be a Stream.");

        byte[]? bytes = null;

        if (stream.CanSeek)
            stream.Position = 0;

        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            bytes = ms.ToArray();
        }

        if (bytes == null || bytes.Length == 0)
            return default;

        encoding ??= Encoding.UTF8;

        string jsonString = encoding.GetString(bytes);

        // Convert the JSON string to a JObject and then to the desired object
        var json = JObject.Parse(jsonString);

        return JsonToObject<T>(json);
    }
}
