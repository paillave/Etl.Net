using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpPostFileValue(Stream stream, string name) : InMemoryFileValue(stream, name)
{
    public override string ToString()
    {
        using var memoryStream = new MemoryStream();
        GetContent().CopyTo(memoryStream);
        byte[] contentBytes = memoryStream.ToArray();
        return System.Text.Encoding.UTF8.GetString(contentBytes);
    }
}
