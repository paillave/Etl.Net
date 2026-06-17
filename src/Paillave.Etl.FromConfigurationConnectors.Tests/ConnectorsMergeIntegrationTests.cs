using System;
using System.Text.Json.Nodes;
using Paillave.Etl.Core;
using Paillave.Etl.Sftp;
using Xunit;

namespace Paillave.Etl.FromConfigurationConnectors.Tests;

/// <summary>
/// Reproduces the exact production workflow from ConnectorsSetupService:
///   1. Several development items each carry their own JSON connector definition.
///   2. They are merged into one JSON string (MergeJsonObjects).
///   3. The merged JSON is fed to GetConnectors() to obtain live providers/processors.
///
/// Uses the exact JSON format that exists in production (Sftp adapter).
/// Sensitive values (Password) are resolved with an identity resolver because
/// the test does not actually open a connection — it only verifies that the
/// parser wires up the correct types from both definition files.
/// </summary>
public class ConnectorsMergeIntegrationTests
{
    // The sensitive-value resolver used throughout these tests.
    // In production this looks up values in connectorsConfiguration.SensitiveValues.
    // Here we return the stored string unchanged (no real SFTP connection is made).
    private static readonly Func<string, string> PassThroughResolver = key => key;

    // Mirrors what ConnectorsSetupService.MergeJsonObjects does for distinct top-level keys.
    private static string MergeDefinitions(params string[] definitions)
    {
        var merged = new JsonObject();
        foreach (var json in definitions)
        {
            var obj = JsonNode.Parse(json)!.AsObject();
            foreach (var (key, value) in obj)
                merged[key] = value?.DeepClone();
        }
        return merged.ToJsonString();
    }

    // -----------------------------------------------------------------------
    // This is definition 1: the exact format from the real codebase.
    // -----------------------------------------------------------------------
    private const string Definition1 = """
    {
        "LocalSFTP": {
            "Type": "Sftp",
            "Connection": {
                "Server": "localhost",
                "PortNumber": 2222,
                "Login": "user",
                "Password": "SftpPassword"
            },
            "Providers": {
                "INPUTPOSITIONS": {
                    "SubFolder": "files/positions",
                    "Name": "InputPositions",
                    "FileNamePattern": "*"
                }
            }
        }
    }
    """;

    // -----------------------------------------------------------------------
    // Definition 2: a second SFTP server managed by a different development item.
    // -----------------------------------------------------------------------
    private const string Definition2 = """
    {
        "RemoteSFTP": {
            "Type": "Sftp",
            "Connection": {
                "Server": "remote.example.com",
                "PortNumber": 22,
                "Login": "etluser",
                "Password": "RemotePassword"
            },
            "Providers": {
                "INPUTDOCUMENTS": {
                    "SubFolder": "files/documents",
                    "Name": "InputDocuments",
                    "FileNamePattern": "*.pdf"
                }
            },
            "Processors": {
                "OUTPUTREPORTS": {
                    "SubFolder": "files/reports",
                    "Name": "OutputReports"
                }
            }
        }
    }
    """;

    // -----------------------------------------------------------------------
    // Core use case: two separate definition files merged and parsed.
    // Providers from both must be reachable with their correct types.
    // -----------------------------------------------------------------------
    [Fact]
    public void GetConnectors_TwoMergedSftpDefinitions_ReturnsBothProvidersAndProcessors()
    {
        var merged = MergeDefinitions(Definition1, Definition2);

        var parser = new ConfigurationFileValueConnectorParser(new SftpProviderProcessorAdapter());
        var connectors = parser.GetConnectors(merged, PassThroughResolver);

        // A NoFileValueConnectors result means schema validation or JSON parsing failed.
        var real = Assert.IsType<FileValueConnectors>(connectors);

        // Providers from both definition files must be present.
        Assert.Contains("INPUTPOSITIONS", real.Providers);
        Assert.Contains("INPUTDOCUMENTS", real.Providers);

        // Processor from definition 2 must be present.
        Assert.Contains("OUTPUTREPORTS", real.Processors);

        // Each must be the real SFTP type — not a No-op fallback.
        Assert.IsType<SftpFileValueProvider>(real.GetProvider("INPUTPOSITIONS"));
        Assert.IsType<SftpFileValueProvider>(real.GetProvider("INPUTDOCUMENTS"));
        Assert.IsType<SftpFileValueProcessor>(real.GetProcessor("OUTPUTREPORTS"));
    }

    // -----------------------------------------------------------------------
    // GetConnectorsSchemaJson is called by the front-end to render the config
    // editor. This is the call that was crashing before the fix.
    // -----------------------------------------------------------------------
    [Fact]
    public void GetConnectorsSchemaJson_WithSftpAdapter_DoesNotThrow()
    {
        var parser = new ConfigurationFileValueConnectorParser(new SftpProviderProcessorAdapter());
        var schemaJson = parser.GetConnectorsSchemaJson();
        Assert.False(string.IsNullOrWhiteSpace(schemaJson));
    }

    // -----------------------------------------------------------------------
    // Full regression: schema generation (editor) then connector parsing (runtime)
    // must both work within the same parser instance, as in production.
    // -----------------------------------------------------------------------
    [Fact]
    public void SchemaAndConnectorParsing_BothWorkInSameSession()
    {
        var parser = new ConfigurationFileValueConnectorParser(new SftpProviderProcessorAdapter());

        // Step 1 — front-end fetches the JSON Schema to build the config editor.
        var schemaJson = parser.GetConnectorsSchemaJson();
        Assert.False(string.IsNullOrWhiteSpace(schemaJson));

        // Step 2 — user saves their config; runtime merges all definitions and parses.
        var merged = MergeDefinitions(Definition1, Definition2);
        var connectors = parser.GetConnectors(merged, PassThroughResolver);

        var real = Assert.IsType<FileValueConnectors>(connectors);
        Assert.Contains("INPUTPOSITIONS", real.Providers);
        Assert.Contains("INPUTDOCUMENTS", real.Providers);
        Assert.Contains("OUTPUTREPORTS", real.Processors);
    }

    // -----------------------------------------------------------------------
    // Single definition (no merge) — baseline to confirm the format is accepted.
    // -----------------------------------------------------------------------
    [Fact]
    public void GetConnectors_SingleSftpDefinition_ReturnsProvider()
    {
        var parser = new ConfigurationFileValueConnectorParser(new SftpProviderProcessorAdapter());
        var connectors = parser.GetConnectors(Definition1, PassThroughResolver);

        var real = Assert.IsType<FileValueConnectors>(connectors);
        Assert.Contains("INPUTPOSITIONS", real.Providers);
        Assert.IsType<SftpFileValueProvider>(real.GetProvider("INPUTPOSITIONS"));
    }

    // -----------------------------------------------------------------------
    // Composite provider defined in the same JSON as the sub-connectors.
    // -----------------------------------------------------------------------
    private const string DefinitionComposite = """
    {
        "AllFiles": {
            "Type": "Composite",
            "Providers": {
                "ALL_INPUTS": {
                    "ProviderCodes": ["INPUTPOSITIONS", "INPUTDOCUMENTS"]
                }
            }
        }
    }
    """;

    [Fact]
    public void GetConnectors_CompositeDefinitionMergedWithSftp_ReturnsCompositeProvider()
    {
        var merged = MergeDefinitions(Definition1, Definition2, DefinitionComposite);

        var parser = new ConfigurationFileValueConnectorParser(new SftpProviderProcessorAdapter());
        var connectors = parser.GetConnectors(merged, PassThroughResolver);

        var real = Assert.IsType<FileValueConnectors>(connectors);
        Assert.Contains("INPUTPOSITIONS", real.Providers);
        Assert.Contains("INPUTDOCUMENTS", real.Providers);
        Assert.Contains("ALL_INPUTS", real.Providers);
        Assert.IsType<Paillave.Etl.Core.CompositeFileValueProvider>(real.GetProvider("ALL_INPUTS"));
    }
}
