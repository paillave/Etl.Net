using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paillave.Etl.Core;
using Paillave.Etl.JsonFile;

namespace Paillave.Etl.JsonFile;

public static partial class JsonEx
{
    public static IStream<T> ParseJson<T>(this IStream<IFileValue> stream, string name)
    {
        return new IFileValueToObjStreamNode<T>(
            name,
            new JsonArgsIFileValueStream<IFileValue> { Stream = stream }
        ).Output;
    }

    public static IStream<JObject> SerializeToJson<T>(this IStream<T> stream, string name)
    {
        return new ObjToJsonStreamNode<T>(
            name,
            new JsonArgsObjectStream<T> { Stream = stream }
        ).Output;
    }
}
