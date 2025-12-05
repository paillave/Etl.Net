using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Paillave.Etl.Core;

namespace Paillave.Etl.FromConfigurationConnectors;

public class ConfigurationAdapterProvider(IConfiguration configuration, ConfigurationFileValueConnectorParser parser)
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
        var fileValueConnectors = parser.GetConnectors(definition, i => i);
        return fileValueConnectors;
    }
    private enum ConnectorType
    {
        Provider,
        Processor
    }
    private static string? BuildConnectorConfiguration(JsonNode? configuration, string code, ConnectorType connectorType)
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
public class ConfigurationMessagingProvider(IConfiguration configuration, IEnumerable<IMessagingProvider> messagingProviders)
{
    public IMessaging GetMessaging(string sectionName)
    {
        IConfigurationSection configurationSection = configuration.GetSection(sectionName);
        var messagingConfiguration = configurationSection.ToJsonNode() ?? throw new InvalidOperationException($"No configuration found for messaging section '{sectionName}'");
        var messagingProvider = messagingProviders.FirstOrDefault(m => m.Name.Equals((string?)messagingConfiguration?["Type"], StringComparison.InvariantCultureIgnoreCase))
           ?? throw new InvalidOperationException($"No messaging provider found for type '{(string?)messagingConfiguration?["Type"]}'");
        return messagingProvider.GetMessaging(messagingConfiguration["Properties"]?? throw new InvalidOperationException("No configuration properties found for messaging provider"));
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
