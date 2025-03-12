using Paillave.Etl.Core;

namespace Paillave.Etl.JsonFile;

public static partial class JsonEx
{
    public static IStream<HttpResponseMessage> Json(
        this IStream<HttpCallArgs> stream,
        string name
    ) => new JsonStreamNode(name, new HttpArgs { Stream = stream }).Output;
}
