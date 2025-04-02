using System;
using System.Collections.Generic;
using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpPostFileValue : InMemoryFileValue<HttpPostFileValueMetadata>
{
    public HttpPostFileValue(Stream stream, string name, HttpPostFileValueMetadata metadata)
        : base(stream, name, metadata) { }

    public override string ToString()
    {
        using (var memoryStream = new MemoryStream())
        {
            GetContent().CopyTo(memoryStream);
            byte[] contentBytes = memoryStream.ToArray();
            return System.Text.Encoding.UTF8.GetString(contentBytes);
        }
    }
}

public class HttpPostFileValueMetadata : HttpFileValueMetadata { }
