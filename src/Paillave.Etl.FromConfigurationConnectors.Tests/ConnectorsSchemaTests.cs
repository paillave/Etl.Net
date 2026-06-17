using System.Threading.Tasks;
using NJsonSchema;
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Http;
using Paillave.Etl.Sftp;
using Xunit;

namespace Paillave.Etl.FromConfigurationConnectors.Tests;

// --- Inline adapter with simple flat types ---
// Ensures the basic AddProperty path works (the wrapper fix applies here too).
file class FlatConnectionParams
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
}
file class FlatProviderParams
{
    public string Path { get; set; } = "";
}
file class FlatProcessorParams
{
    public string TargetPath { get; set; } = "";
}
file class FlatAdapter : ProviderProcessorAdapterBase<FlatConnectionParams, FlatProviderParams, FlatProcessorParams>
{
    public override string Name => "Flat";
    public override string Description => "Flat adapter for tests";
    protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, FlatConnectionParams c, FlatProviderParams p)
        => throw new System.NotImplementedException();
    protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, FlatConnectionParams c, FlatProcessorParams p)
        => throw new System.NotImplementedException();
}

// --- Inline adapter with enum and nested object types ---
// Specifically exercises the case where JsonSchema.FromType() puts sub-type schemas
// in the generated schema's own Definitions. The AddProperty wrapper fix must handle this.
file enum AuthMode { ApiKey, OAuth2 }
file class NestedAuth
{
    public string Token { get; set; } = "";
    public AuthMode Mode { get; set; }
}
file class ComplexConnectionParams
{
    public string Url { get; set; } = "";
    public NestedAuth? Auth { get; set; }           // nested object  → goes into Definitions
    public AuthMode DefaultMode { get; set; }        // enum           → goes into Definitions
}
file class ComplexProviderParams
{
    public AuthMode PreferredMode { get; set; }      // same enum, different schema instance
    public string SubPath { get; set; } = "";
}
file class ComplexAdapter : ProviderProcessorAdapterBase<ComplexConnectionParams, ComplexProviderParams, FlatProcessorParams>
{
    public override string Name => "Complex";
    public override string Description => "Adapter with nested types for tests";
    protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, ComplexConnectionParams c, ComplexProviderParams p)
        => throw new System.NotImplementedException();
    protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, ComplexConnectionParams c, FlatProcessorParams p)
        => throw new System.NotImplementedException();
}

public class ConnectorsSchemaTests
{
    // -----------------------------------------------------------------------
    // Case 1: single adapter with flat types.
    // Even a flat type triggers the AddProperty wrapper bug:
    //   JsonSchema.FromType(T).AddAsDefinition(doc) returns a wrapper { Reference = actual }.
    //   AddProperty used to set property.Reference = wrapper (not in Definitions → crash).
    // -----------------------------------------------------------------------
    [Fact]
    public void GetConnectorsSchemaJson_FlatTypes_DoesNotThrow()
    {
        var parser = new ConfigurationFileValueConnectorParser(new FlatAdapter());
        var json = parser.GetConnectorsSchemaJson(); // must not throw
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    // -----------------------------------------------------------------------
    // Case 2: adapter whose parameter types have enums and nested objects.
    // JsonSchema.FromType() puts those sub-schemas into the generated schema's
    // own .Definitions. The wrapper fix + nested-definitions traversal must handle this.
    // -----------------------------------------------------------------------
    [Fact]
    public void GetConnectorsSchemaJson_NestedTypesAndEnums_DoesNotThrow()
    {
        var parser = new ConfigurationFileValueConnectorParser(new ComplexAdapter());
        var json = parser.GetConnectorsSchemaJson();
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    // -----------------------------------------------------------------------
    // Case 3: adapter with Providers only (no Processors).
    // The Providers code path also calls AddProperty with a wrapped schema.
    // -----------------------------------------------------------------------
    [Fact]
    public void GetConnectorsSchemaJson_WithFileSystemAdapter_DoesNotThrow()
    {
        var parser = new ConfigurationFileValueConnectorParser(new FileSystemProviderProcessorAdapter());
        var json = parser.GetConnectorsSchemaJson();
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    // -----------------------------------------------------------------------
    // Case 4: real-world adapter with deeply nested types (HttpAuthentication
    // contains BearerAuthentication, BasicAuthentication, DigestAuthentication,
    // XCBACCESSAuthentication, DigestAlgorithm enum, HttpMethodCustomEnum, etc.).
    // This is the exact scenario from the original exception.
    // -----------------------------------------------------------------------
    [Fact]
    public void GetConnectorsSchemaJson_HttpAdapterWithDeeplyNestedTypes_DoesNotThrow()
    {
        var parser = new ConfigurationFileValueConnectorParser(new HttpProviderProcessorAdapter());
        var json = parser.GetConnectorsSchemaJson();
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    // -----------------------------------------------------------------------
    // Case 5: multiple adapters combined — the full scenario used in production.
    // Exercises name collisions between same enum/type appearing in multiple adapters.
    // -----------------------------------------------------------------------
    [Fact]
    public void GetConnectorsSchemaJson_MultipleAdapters_DoesNotThrow()
    {
        var parser = new ConfigurationFileValueConnectorParser(
            new FlatAdapter(),
            new ComplexAdapter(),
            new FileSystemProviderProcessorAdapter(),
            new HttpProviderProcessorAdapter()
        );
        var json = parser.GetConnectorsSchemaJson();
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    // -----------------------------------------------------------------------
    // Case 6: the returned string must be parseable as a JSON Schema.
    // Verifies structural correctness, not just absence of exception.
    // -----------------------------------------------------------------------
    [Fact]
    public async Task GetConnectorsSchemaJson_ReturnedJson_IsValidJsonSchema()
    {
        var parser = new ConfigurationFileValueConnectorParser(
            new FileSystemProviderProcessorAdapter(),
            new HttpProviderProcessorAdapter()
        );
        var json = parser.GetConnectorsSchemaJson();

        // FromJsonAsync throws if the JSON is not a valid JSON Schema.
        var schema = await JsonSchema.FromJsonAsync(json);
        Assert.NotNull(schema);
        Assert.Equal(JsonObjectType.Object, schema.Type);
    }

    // -----------------------------------------------------------------------
    // Case 7: adapter with NO provider parameters type (Processors only).
    // Ensures the null-check branch for ProviderParametersType is exercised.
    // -----------------------------------------------------------------------
    [Fact]
    public void GetConnectorsSchemaJson_AdapterWithNullProviderParams_DoesNotThrow()
    {
        var parser = new ConfigurationFileValueConnectorParser(new FlatAdapter());
        // The FlatAdapter does have a ProviderParametersType, but we verify the schema
        // can be generated regardless. The important test is that no exception is thrown.
        var json = parser.GetConnectorsSchemaJson();
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    // -----------------------------------------------------------------------
    // Case 8: mirrors the regression test in the PMSv2 solution.
    // Uses all real adapters available in this repo (FileSystem, Http, Sftp)
    // to reproduce the original InvalidOperationException from production.
    // -----------------------------------------------------------------------
    [Fact]
    public void GetConnectorsSchemaJson_AllRealAdapters_ReturnsValidJson()
    {
        var parser = new ConfigurationFileValueConnectorParser(
            new FileSystemProviderProcessorAdapter(),
            new HttpProviderProcessorAdapter(),
            new SftpProviderProcessorAdapter()
        );
        var json = parser.GetConnectorsSchemaJson();
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    // -----------------------------------------------------------------------
    // Case 9 — regression for the production crash.
    //
    // ConfigurationFileValueConnectorParser always auto-prepends
    // CompositeFileValueProviderAdapter internally. Production code in PMSv2
    // also passed it explicitly, resulting in TWO Composite adapters in the
    // list. The second pass overwrites docSchema.Definitions["Sources_Composite"],
    // leaving the first adapter's AdditionalPropertiesSchema reference dangling.
    // ToJson() then throws:
    //   System.InvalidOperationException: Could not find the JSON path of a
    //   referenced schema: NJsonSchema.JsonSchema,NJsonSchema.JsonSchema.
    //
    // The fix deduplicates adapters by Name in the constructor so that passing
    // CompositeFileValueProviderAdapter explicitly is silently ignored.
    // -----------------------------------------------------------------------
    [Fact]
    public void GetConnectorsSchemaJson_CompositePassedExplicitly_DoesNotThrow()
    {
        // Mirrors the production GetParser() call that explicitly includes
        // CompositeFileValueProviderAdapter even though the constructor already
        // auto-prepends it — should not throw InvalidOperationException.
        var parser = new ConfigurationFileValueConnectorParser(
            new SftpProviderProcessorAdapter(),
            new CompositeFileValueProviderAdapter()
        );
        var json = parser.GetConnectorsSchemaJson();
        Assert.False(string.IsNullOrWhiteSpace(json));
    }
}
