using NJsonSchema;

namespace Paillave.Etl.FromConfigurationConnectors
{
    internal static class JsonSchemaEx
    {
        public static JsonSchema AddStringProperty(this JsonSchema schema, string name, bool required, params string[] possibleValues)
        {
            var typeProperty = new JsonSchemaProperty
            {
                Type = JsonObjectType.String,
                AllowAdditionalProperties = false,
            };
            foreach (var possibleValue in possibleValues)
                typeProperty.Enumeration.Add(possibleValue);
            schema.Properties.Add(name, typeProperty);
            if (required)
                schema.RequiredProperties.Add(name);
            return schema;
        }
        // public static JsonSchema Reference(this JsonSchema schema) => new JsonSchema { Reference = schema };
        public static JsonSchema AddProperty(this JsonSchema schema, string name, JsonSchema schemaProperty, bool required)
        {
            if (schemaProperty.Reference != null)
            {
                schema.Properties.Add(name, new JsonSchemaProperty { Reference = schemaProperty });
            }
            else
            {
                schema.Properties.Add(name, new JsonSchemaProperty
                {
                    Type = JsonObjectType.Object,
                    Item = schemaProperty
                });
            }
            if (required)
                schema.RequiredProperties.Add(name);
            return schema;
        }
        public static JsonSchema AddProperty(this JsonSchema schema, string name, JsonSchemaProperty property, bool required)
        {
            schema.Properties.Add(name, property);
            if (required)
                schema.RequiredProperties.Add(name);
            return schema;
        }
        public static JsonSchema AddArrayProperty(this JsonSchema schema, string name, JsonSchema schemaInArray, bool required)
        {
            schema.Properties.Add(name, new JsonSchemaProperty
            {
                Type = JsonObjectType.Array,
                Item = schemaInArray,
                AllowAdditionalProperties = false,
                MinItems = required ? 1 : 0
            });
            if (required)
                schema.RequiredProperties.Add(name);
            return schema;
        }
        public static JsonSchema AddAnyOfProperty(this JsonSchema schema, string name, bool required, params JsonSchema[] values)
        {
            var enumProperty = new JsonSchemaProperty();
            foreach (var value in values)
            {
                enumProperty.AnyOf.Add(value);
            }
            schema.Properties.Add(name, enumProperty);
            if (required)
                schema.RequiredProperties.Add(name);
            return schema;
        }
        public static JsonSchema CreateObject(string title)
   => new JsonSchema
   {
       Title = title,
       Type = JsonObjectType.Object,
       AllowAdditionalProperties = false,
   };
        public static JsonSchema CreateArray(string title, JsonSchema schema, int minItems = 0)
            => new JsonSchema
            {
                Title = title,
                Type = JsonObjectType.Array,
                AllowAdditionalItems = false,
                Item = schema,
                MinItems = minItems
            };
        public static JsonSchema CreateDictionary(string title, JsonSchema schema)
            => new JsonSchema
            {
                Title = title,
                Type = JsonObjectType.Object,
                AllowAdditionalProperties = true,
                AdditionalPropertiesSchema = schema
            };
        public static JsonSchema CreateAnyOf(string title, params JsonSchema[] schemas)
            => new JsonSchema
            {
                Title = title,
            }.AddAnyOf(schemas);
        public static JsonSchema AddAnyOf(this JsonSchema schema, params JsonSchema[] schemas)
        {
            foreach (var s in schemas)
                schema.AnyOf.Add(s);
            return schema;
        }
        public static JsonSchema CreateStringType(string title)
            => new JsonSchema
            {
                Title = title,
                Type = JsonObjectType.String
            };
        public static JsonSchema SetAsDictionary(this JsonSchema schema, JsonSchema collectedSchema)
        {
            schema.Type = JsonObjectType.Object;
            schema.AllowAdditionalProperties = true;
            schema.AdditionalPropertiesSchema = collectedSchema;
            return schema;
        }
        public static JsonSchema AddAsDefinition(this JsonSchema schema, JsonSchema targetSchema)
        {
            targetSchema.Definitions[schema.Title] = schema;

            return new JsonSchema { Reference = schema };
        }
    }
}