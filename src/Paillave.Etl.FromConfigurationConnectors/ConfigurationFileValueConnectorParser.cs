using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Paillave.Etl.Connector;

namespace Paillave.Etl.FromConfigurationConnectors
{
    public class ConfigurationFileValueConnectorParser
    {
        private IProviderProcessorAdapter[] _providerProcessorAdapter;
        public ConfigurationFileValueConnectorParser(params IProviderProcessorAdapter[] providerProcessorAdapter)
        {
            _providerProcessorAdapter = providerProcessorAdapter;
        }
        public IFileValueConnectors GetConnectors(string jsonConfig)
        {
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
            Dictionary<string, IProviderProcessorAdapter> adapterDictionary = _providerProcessorAdapter.ToDictionary(i => i.Name);
            JObject o = JObject.Parse(jsonConfig);
            var elts = o.Properties()
                .Where(p => p.Path != "$schema")
                .Select(i => ParseConnections(i, adapterDictionary));
            var processors = elts.SelectMany(i => i.processors).ToDictionary(i => i.Code);
            var providers = elts.SelectMany(i => i.providers).ToDictionary(i => i.Code);
            return new FileValueConnectors(providers, processors);
        }
        private (List<IFileValueProvider> providers, List<IFileValueProcessor> processors) ParseConnections(JProperty property, Dictionary<string, IProviderProcessorAdapter> adapterDictionary)
        {
            var connectionNode = (JObject)property.Value;
            string connectionName = property.Name;
            string connectionTypeCode = (string)connectionNode["Type"];
            var adapter = adapterDictionary[connectionTypeCode];
            var connectionParameters = connectionNode["Connection"].ToObject(adapter.ConnectionParametersType);
            var providersNode = (JObject)connectionNode["Providers"];
            var providers = providersNode == null ? new List<IFileValueProvider>() : providersNode.Properties()
                .Select(i => ParseProvider(connectionName, connectionParameters, adapter, i))
                .ToList();
            var processorsNode = (JObject)connectionNode["Processors"];
            var processors = processorsNode == null ? new List<IFileValueProcessor>() : processorsNode.Properties()
                .Select(i => ParseProcessor(connectionName, connectionParameters, adapter, i))
                .ToList();
            return (providers, processors);
        }
        private IFileValueProcessor ParseProcessor(string connectionName, object connectionParameters, IProviderProcessorAdapter adapter, JProperty i)
        {
            var code = i.Name;
            var value = i.Value;
            var connectorNode = (JObject)i.Value;
            var name = ((string)connectorNode["Name"]) ?? code;
            return adapter.CreateProcessor(code, name, connectionName, connectionParameters, connectorNode.ToObject(adapter.ProcessorParametersType));
        }
        private IFileValueProvider ParseProvider(string connectionName, object connectionParameters, IProviderProcessorAdapter adapter, JProperty i)
        {
            var code = i.Name;
            var value = i.Value;
            var connectorNode = (JObject)i.Value;
            var name = ((string)connectorNode["Name"]) ?? code;
            return adapter.CreateProvider(code, name, connectionName, connectionParameters, connectorNode.ToObject(adapter.ProviderParametersType));
        }
        public string GetConnectorsSchemaJson() => GetConnectorsSchema().ToJson();
        private static JsonSchema CreateAddonAdapters(JsonSchema docSchema, string title, IProviderProcessorAdapter[] dictionary)
        {
            var schema = new JsonSchema
            {
                Title = title,
                Type = JsonObjectType.Object
            }
                .AddAnyOfProperty("Type", false, dictionary.Select(k =>
                  {
                      var s = new JsonSchema();
                      s.Enumeration.Add(k.Name);
                      s.Description = k.Description;
                      return s;
                  }).ToArray());
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

            var connectionSchema = CreateAddonAdapters(docSchema, "Connection", _providerProcessorAdapter)
                .AddAsDefinition(docSchema);

            docSchema
                .SetAsDictionary(connectionSchema);

            return docSchema;
        }
    }
}