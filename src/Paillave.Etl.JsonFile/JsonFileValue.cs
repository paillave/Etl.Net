using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paillave.Etl.Core;

namespace Paillave.Etl.JsonFile;

public class JsonFileValue : FileValueBase<JsonFileValueMetadata>
{
    public override string Name { get; }

    JObject Content { get; set; }

    public JsonFileValue(
        string name,
        JObject content,
        string connectorCode,
        string connectionName,
        string connectorName
    )
        : base(
            new JsonFileValueMetadata
            {
                ConnectorCode = connectorCode,
                ConnectionName = connectionName,
                ConnectorName = connectorName,
            }
        )
    {
        Content = content;
        Name = name;
    }

    public JsonFileValue(string genericName, JObject content)
        : base(
            new JsonFileValueMetadata
            {
                ConnectorCode = genericName,
                ConnectionName = genericName,
                ConnectorName = genericName,
            }
        )
    {
        Content = content;
        Name = genericName;
    }

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

public class JsonFileValueMetadata : FileValueMetadataBase { }
