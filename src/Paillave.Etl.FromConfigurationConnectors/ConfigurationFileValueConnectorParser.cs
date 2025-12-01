using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Paillave.Etl.Core;

namespace Paillave.Etl.FromConfigurationConnectors
{
    public class ConfigurationFileValueConnectorParser(params IProviderProcessorAdapter[] providerProcessorAdapter)
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
                if (validator.Where(v => !(new[] { NJsonSchema.Validation.ValidationErrorKind.NoAdditionalPropertiesAllowed, NJsonSchema.Validation.ValidationErrorKind.AdditionalPropertiesNotValid }.Contains(v.Kind) && v.Path == "#/$schema")).Count() > 0)
                    return new NoFileValueConnectors();
            }
            catch
            {
                return new NoFileValueConnectors();
            }
            Dictionary<string, IProviderProcessorAdapter> adapterDictionary = providerProcessorAdapter.ToDictionary(i => i.Name);
            JObject o = JObject.Parse(jsonConfig);
            var elts = o.Properties()
                .Where(p => p.Path != "$schema")
                .Select(i => ParseConnections(i, adapterDictionary, resolveSensitiveValue));
            var processors = elts.SelectMany(i => i.processors).ToDictionary(i => i.Code);
            var providers = elts.SelectMany(i => i.providers).ToDictionary(i => i.Code);
            return new FileValueConnectors(providers, processors);
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
        private static JsonSchema CreateAddonAdapters(JsonSchema docSchema, string title, IProviderProcessorAdapter[] dictionary)
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
                schema.AnyOf.Add(CreateAddonAdapter(docSchema, item));
            return schema;
        }
        private static JsonSchema CreateAddonAdapter(JsonSchema docSchema, IProviderProcessorAdapter inputOutputAdapter)
        {
            var connectionParameterSchema = JsonSchema
                .FromType(inputOutputAdapter.ConnectionParametersType)
                .AddAsDefinition(docSchema);

            var adapterSchema = JsonSchemaEx
                .CreateObject($"Adapter_{inputOutputAdapter.Name}")
                .AddProperty("Type", new JsonSchemaProperty { Pattern = $"^{inputOutputAdapter.Name}$" }, true)
                .AddProperty("Connection", connectionParameterSchema, false);

            if (inputOutputAdapter.ProviderParametersType != null)
            {
                var providerParameterSchema = JsonSchema
                    .FromType(inputOutputAdapter.ProviderParametersType)
                    .AddStringProperty("Name", false);
                var providersSchema = JsonSchemaEx
                    .CreateDictionary($"Sources_{inputOutputAdapter.Name}", providerParameterSchema)
                    .AddAsDefinition(docSchema);
                adapterSchema
                    .AddProperty("Providers", providersSchema, false);
            }

            if (inputOutputAdapter.ProcessorParametersType != null)
            {
                var processorParameterSchema = JsonSchema
                    .FromType(inputOutputAdapter.ProcessorParametersType)
                    .AddStringProperty("Name", false);
                var processorsSchema = JsonSchemaEx
                    .CreateDictionary($"Processes_{inputOutputAdapter.Name}", processorParameterSchema)
                    .AddAsDefinition(docSchema);
                adapterSchema
                    .AddProperty("Processors", processorsSchema, false);
            }

            return adapterSchema.AddAsDefinition(docSchema);
        }
        public JsonSchema GetConnectorsSchema()
        {
            var docSchema = JsonSchemaEx.CreateObject("Document");

            var connectionSchema = CreateAddonAdapters(docSchema, "Connection", providerProcessorAdapter)
                .AddAsDefinition(docSchema);

            docSchema
                .SetAsDictionary(connectionSchema);

            return docSchema;
        }
    }
}