using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Paillave.Etl.Core;

namespace Paillave.Etl.JsonFile;

public class JsonFileValue : FileValueBase
{
    public override string Name { get; }

    JObject Content { get; set; }

    public JsonFileValue(string name, JObject content)
        => (Content, Name) = (content, name);

    public override StreamWithResource OpenContent() => new(GetContent());

    public override Stream GetContent()
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(Content.ToString());
        return new MemoryStream(byteArray);
    }

    protected override void DeleteFile()
    {
        // Stub
    }
}
