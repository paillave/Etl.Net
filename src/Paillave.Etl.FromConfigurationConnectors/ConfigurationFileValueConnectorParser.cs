using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;
using Paillave.Etl.Core;

namespace Paillave.Etl.FromConfigurationConnectors;

public class ConfigurationFileValueConnectorParser(params IProviderProcessorAdapter[] providerProcessorAdapters)
{
    public IFileValueConnectors GetConnectors(string jsonConfig, Func<string, string> resolveSensitiveValue)
    {
        if (string.IsNullOrWhiteSpace(jsonConfig))
        {
            return new NoFileValueConnectors();
        }
        try
        {
            var validator = new NJsonSchema.Validation.JsonSchemaValidator().Validate(jsonConfig, this.GetConnectorsSchema());
            if (validator.Any(v => !(new[] { NJsonSchema.Validation.ValidationErrorKind.NoAdditionalPropertiesAllowed, NJsonSchema.Validation.ValidationErrorKind.AdditionalPropertiesNotValid }.Contains(v.Kind) && v.Path == "#/$schema")))
                return new NoFileValueConnectors();
        }
        catch
        {
            return new NoFileValueConnectors();
        }
        Dictionary<string, IProviderProcessorAdapter> adapterDictionary = providerProcessorAdapters.ToDictionary(i => i.Name);
        JObject o = JObject.Parse(jsonConfig);
        var elts = o.Properties()
            .Where(p => p.Path != "$schema")
            .Select(i => ParseConnections(i, adapterDictionary, resolveSensitiveValue));
        var processors = elts.SelectMany(i => i.processors).ToDictionary(i => i.Code);
        var providers = elts.SelectMany(i => i.providers).ToDictionary(i => i.Code);
        return new FileValueConnectors(providers, processors);
    }
    public IFileValueProvider GetProvider(IConfigurationSection configurationSection)
    {
        var type = configurationSection.GetSection("Type").Value;
        var providerProcessorAdapter = providerProcessorAdapters.Single(a => a.Name.Equals(type, StringComparison.InvariantCultureIgnoreCase));
        var connectionConfiguration = configurationSection.GetSection("Connection");
        var connectionParameters = connectionConfiguration.Get(providerProcessorAdapter.ConnectionParametersType);
        var providerSection = configurationSection.GetSection("Provider");
        var providerParameters = providerSection.Get(providerProcessorAdapter.ProviderParametersType) ?? Activator.CreateInstance(providerProcessorAdapter.ProviderParametersType);
        return providerProcessorAdapter.CreateProvider("code", "name", "cnx", connectionParameters, providerParameters);
    }
    public IFileValueProcessor GetProcessor(IConfigurationSection configurationSection)
    {
        var type = configurationSection.GetSection("Type").Value;
        var providerProcessorAdapter = providerProcessorAdapters.Single(a => a.Name.Equals(type, StringComparison.InvariantCultureIgnoreCase));
        var connectionConfiguration = configurationSection.GetSection("Connection");
        var connectionParameters = connectionConfiguration.Get(providerProcessorAdapter.ConnectionParametersType);
        var processorSection = configurationSection.GetSection("Processor");
        var processorParameters = processorSection.Get(providerProcessorAdapter.ProcessorParametersType) ?? Activator.CreateInstance(providerProcessorAdapter.ProcessorParametersType);
        return providerProcessorAdapter.CreateProcessor("code", "name", "cnx", connectionParameters, processorParameters);
    }
    private (List<IFileValueProvider> providers, List<IFileValueProcessor> processors) ParseConnections(JProperty property, Dictionary<string, IProviderProcessorAdapter> adapterDictionary, Func<string, string> resolveSensitiveValue)
    {
        var connectionNode = (JObject)property.Value;
        string connectionName = property.Name;
        string connectionTypeCode = (string)connectionNode["Type"];
        var adapter = adapterDictionary[connectionTypeCode];
        var connectionParameters = connectionNode["Connection"].ToObject(adapter.ConnectionParametersType);
        ResolveSensitiveProperties(connectionParameters, resolveSensitiveValue);
        var providersNode = (JObject)connectionNode["Providers"];
        var providers = providersNode == null ? new List<IFileValueProvider>() : [.. providersNode.Properties().Select(i => ParseProvider(connectionName, connectionParameters, adapter, i, resolveSensitiveValue))];
        var processorsNode = (JObject)connectionNode["Processors"];
        var processors = processorsNode == null ? [] : processorsNode.Properties()
            .Select(i => ParseProcessor(connectionName, connectionParameters, adapter, i, resolveSensitiveValue))
            .ToList();
        return (providers, processors);
    }
    private void ResolveSensitiveProperties(object obj, Func<string, string> resolveSensitiveValue)
    {
        if (obj == null) return;
        var props = obj.GetType().GetProperties().Where(p => p.CanRead && p.CanWrite);
        foreach (var prop in props)
        {
            var isSensitive = Attribute.IsDefined(prop, typeof(SensitiveAttribute));
            if (isSensitive)
            {
                var currentValue = prop.GetValue(obj) as string;
                if (!string.IsNullOrWhiteSpace(currentValue))
                {
                    var resolvedValue = resolveSensitiveValue(currentValue);
                    prop.SetValue(obj, resolvedValue);
                }
            }
        }
    }
    private IFileValueProcessor ParseProcessor(string connectionName, object connectionParameters, IProviderProcessorAdapter adapter, JProperty i, Func<string, string> resolveSensitiveValue)
    {
        var code = i.Name;
        var value = i.Value;
        var connectorNode = (JObject)i.Value;
        var name = ((string)connectorNode["Name"]) ?? code;
        var parameters = connectorNode.ToObject(adapter.ProcessorParametersType);
        ResolveSensitiveProperties(parameters, resolveSensitiveValue);
        return adapter.CreateProcessor(code, name, connectionName, connectionParameters, parameters);
    }
    private IFileValueProvider ParseProvider(string connectionName, object connectionParameters, IProviderProcessorAdapter adapter, JProperty i, Func<string, string> resolveSensitiveValue)
    {
        var code = i.Name;
        var value = i.Value;
        var connectorNode = (JObject)i.Value;
        var name = ((string)connectorNode["Name"]) ?? code;
        var parameters = connectorNode.ToObject(adapter.ProviderParametersType);
        ResolveSensitiveProperties(parameters, resolveSensitiveValue);
        return adapter.CreateProvider(code, name, connectionName, connectionParameters, parameters);
    }
    public string GetConnectorsSchemaJson() => GetConnectorsSchema().ToJson();
    private static JsonSchema CreateAddonAdapters(JsonSchema docSchema, string title, IProviderProcessorAdapter[] dictionary, JsonSchemaGenerator generator, JsonSchemaResolver resolver)
    {
        var schema = new JsonSchema
        {
            Title = title,
            Type = JsonObjectType.Object
        }
            .AddAnyOfProperty("Type", false, [.. dictionary.Select(k =>
              {
                  var s = new JsonSchema();
                  s.Enumeration.Add(k.Name);
                  s.Description = k.Description;
                  return s;
              })]);
        foreach (var item in dictionary)
            schema.AnyOf.Add(CreateAddonAdapter(docSchema, item, generator, resolver));
        return schema;
    }
    private static JsonSchema CreateAddonAdapter(JsonSchema docSchema, IProviderProcessorAdapter inputOutputAdapter, JsonSchemaGenerator generator, JsonSchemaResolver resolver)
    {
        // Use a shared resolver so each .NET type maps to exactly one JsonSchema instance.
        // Multiple adapters referencing the same type (e.g. an enum) would otherwise produce
        // independent instances; only one would land in Definitions, leaving the others as
        // dangling references that ToJson() cannot resolve.
        var connectionParameterSchema = new JsonSchema
        {
            Reference = generator.Generate(inputOutputAdapter.ConnectionParametersType, resolver)
        };

        var adapterSchema = JsonSchemaEx
            .CreateObject($"Adapter_{inputOutputAdapter.Name}")
            .AddProperty("Type", new JsonSchemaProperty { Pattern = $"^{inputOutputAdapter.Name}$" }, true)
            .AddProperty("Connection", connectionParameterSchema, false);

        if (inputOutputAdapter.ProviderParametersType != null)
        {
            var providerParameterSchema = generator.Generate(inputOutputAdapter.ProviderParametersType, resolver);
            if (!providerParameterSchema.Properties.ContainsKey("Name"))
                providerParameterSchema.AddStringProperty("Name", false);
            var providersSchema = JsonSchemaEx
                .CreateDictionary($"Sources_{inputOutputAdapter.Name}", new JsonSchema { Reference = providerParameterSchema })
                .AddAsDefinition(docSchema);
            adapterSchema
                .AddProperty("Providers", providersSchema, false);
        }

        if (inputOutputAdapter.ProcessorParametersType != null)
        {
            var processorParameterSchema = generator.Generate(inputOutputAdapter.ProcessorParametersType, resolver);
            if (!processorParameterSchema.Properties.ContainsKey("Name"))
                processorParameterSchema.AddStringProperty("Name", false);
            var processorsSchema = JsonSchemaEx
                .CreateDictionary($"Processes_{inputOutputAdapter.Name}", new JsonSchema { Reference = processorParameterSchema })
                .AddAsDefinition(docSchema);
            adapterSchema
                .AddProperty("Processors", processorsSchema, false);
        }

        return adapterSchema.AddAsDefinition(docSchema);
    }
    public JsonSchema GetConnectorsSchema()
    {
        var docSchema = JsonSchemaEx.CreateObject("Document");
        var settings = new SystemTextJsonSchemaGeneratorSettings();
        var generator = new JsonSchemaGenerator(settings);
        var resolver = new JsonSchemaResolver(docSchema, settings);

        var connectionSchema = CreateAddonAdapters(docSchema, "Connection", providerProcessorAdapters, generator, resolver)
            .AddAsDefinition(docSchema);

        docSchema
            .SetAsDictionary(connectionSchema);

        return docSchema;
    }
}