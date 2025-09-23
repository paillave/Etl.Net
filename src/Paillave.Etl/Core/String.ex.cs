using System.IO;
using System.Linq;

namespace Paillave.Etl.Core;

public static class StringEx
{
    public static Stream ToStream(this string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
    public static string ConcatenatePath(params string?[] segments)
    {
        return string.Join("/", segments.Where(i => !string.IsNullOrWhiteSpace(i))).Replace("//", "/");
    }
}
