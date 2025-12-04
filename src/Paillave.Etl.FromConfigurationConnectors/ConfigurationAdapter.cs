using System;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Paillave.Etl.Core;

namespace Paillave.Etl.FromConfigurationConnectors;

public class ConfigurationAdapter(IConfiguration configuration, ConfigurationFileValueConnectorParser parser, Func<string, string> resolveSensitiveValues)
{
    private const string code = "DEVITEMSOURCE";
    public IFileValueProvider GetFileValueProvider(string sectionName)
        => CreateFileValueConnectors(sectionName, ConnectorType.Provider).GetProvider(code);
    public IFileValueProcessor GetFileValueProcessor(string sectionName)
        => CreateFileValueConnectors(sectionName, ConnectorType.Processor).GetProcessor(code);

    private IFileValueConnectors CreateFileValueConnectors(string sectionName, ConnectorType connectorType)
    {
        IConfigurationSection configurationSection = configuration.GetSection(sectionName);
        var section = configurationSection.ToJsonNode();
        var definition = BuildConnectorConfiguration(section, code, connectorType) ?? throw new InvalidOperationException("No configuration found for a connector");
        var fileValueConnectors = parser.GetConnectors(definition, resolveSensitiveValues);
        return fileValueConnectors;
    }
    private enum ConnectorType
    {
        Provider,
        Processor
    }
    private string? BuildConnectorConfiguration(JsonNode? configuration, string code, ConnectorType connectorType)
    {
        if (configuration is null)
            return null;

        var obj = new JsonObject()
        {
            ["src"] = new JsonObject()
            {
                ["Type"] = configuration?["Type"]?.DeepClone(),
                ["Connection"] = configuration?["Connection"]?.DeepClone()
            }
        };
        switch (connectorType)
        {
            case ConnectorType.Provider:
                obj["src"]!["Providers"] = new JsonObject()
                {
                    [code] = configuration?["Provider"]?.DeepClone() ?? new JsonObject()
                };
                break;
            case ConnectorType.Processor:
                obj["src"]!["Processors"] = new JsonObject()
                {
                    [code] = configuration?["Processor"]?.DeepClone() ?? new JsonObject()
                };
                break;
        }
        return obj.ToJsonString();
    }
}
public static class ConfigurationJsonExtensions
{
    public static JsonNode? ToJsonNode(this IConfigurationSection section)
    {
        if (!section.Exists()) return null;

        var children = section.GetChildren().ToList();
        if (children.Count == 0)
        {
            return section.Value is null ? JsonValue.Create((string?)null) : JsonValue.Create(section.Value);
        }

        if (children.All(c => int.TryParse(c.Key, out _)))
        {
            var arr = new JsonArray();
            foreach (var child in children.OrderBy(c => int.Parse(c.Key)))
                arr.Add(child.ToJsonNode());
            return arr;
        }

        var obj = new JsonObject();
        foreach (var child in children)
            obj[child.Key] = child.ToJsonNode();
        return obj;
    }
}
