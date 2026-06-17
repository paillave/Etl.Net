using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Xunit;

namespace Paillave.Etl.Tests;

public class CompositeFileValueProviderTests
{
    private class FakeFileValue(string name) : FileValueBase
    {
        public override string Name => name;
        protected override void DeleteFile() { }
        public override Stream GetContent() => new MemoryStream();
        public override StreamWithResource OpenContent() => new(new MemoryStream());
    }

    private class FakeProvider(string code, ProcessImpact impact = ProcessImpact.Light, params string[] fileNames) : IFileValueProvider
    {
        public string Code => code;
        public ProcessImpact PerformanceImpact => impact;
        public ProcessImpact MemoryFootPrint => impact;
        public bool WasTested { get; private set; }

        public void Test() => WasTested = true;

        public void Provide(object? input, System.Action<IFileValue, FileReference> pushFileValue, CancellationToken cancellationToken)
        {
            foreach (var name in fileNames)
            {
                if (cancellationToken.IsCancellationRequested) break;
                pushFileValue(new FakeFileValue(name), new FileReference(name, code, null));
            }
        }

        public IFileValue Provide(JsonNode? fileSpecific) => throw new System.NotSupportedException();
        public Stream Open(JsonNode fileSpecific) => throw new System.NotSupportedException();

        public async IAsyncEnumerable<IFileValue> ProvideAsync(object? input = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var name in fileNames)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new FakeFileValue(name);
                await Task.Yield();
            }
        }

        public async IAsyncEnumerable<FileReference> EnumerateAsync(object? input = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var name in fileNames)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new FileReference(name, code, null);
                await Task.Yield();
            }
        }
    }

    // --- Adapter contract ---

    [Fact]
    public void Adapter_ProcessorParametersType_IsNull()
    {
        var adapter = new CompositeFileValueProviderAdapter();
        Assert.Null(adapter.ProcessorParametersType);
    }

    [Fact]
    public void Adapter_ProviderParametersType_IsCompositeProviderParameters()
    {
        var adapter = new CompositeFileValueProviderAdapter();
        Assert.Equal(typeof(CompositeProviderParameters), adapter.ProviderParametersType);
    }

    [Fact]
    public void Adapter_Name_IsComposite()
    {
        var adapter = new CompositeFileValueProviderAdapter();
        Assert.Equal("Composite", adapter.Name);
    }

    // --- CompositeFileValueProvider behavior ---

    [Fact]
    public void Provider_Test_CallsTestOnAllUnderlyingProviders()
    {
        var p1 = new FakeProvider("A", ProcessImpact.Light, "a1.csv");
        var p2 = new FakeProvider("B", ProcessImpact.Light, "b1.csv");
        var connectors = new FileValueConnectors();
        connectors.Register(p1).Register(p2);

        var composite = new CompositeFileValueProvider("C", connectors, "A", "B");
        composite.Test();

        Assert.True(p1.WasTested);
        Assert.True(p2.WasTested);
    }

    [Fact]
    public void Provider_Provide_AggregatesFilesInOrder()
    {
        var connectors = new FileValueConnectors();
        connectors
            .Register(new FakeProvider("A", ProcessImpact.Light, "a1.csv", "a2.csv"))
            .Register(new FakeProvider("B", ProcessImpact.Light, "b1.csv"));

        var composite = new CompositeFileValueProvider("C", connectors, "A", "B");

        var collected = new List<string>();
        composite.Provide(null, (fv, _) => collected.Add(fv.Name), CancellationToken.None);

        Assert.Equal(["a1.csv", "a2.csv", "b1.csv"], collected);
    }

    [Fact]
    public async Task Provider_ProvideAsync_AggregatesFilesInOrder()
    {
        var connectors = new FileValueConnectors();
        connectors
            .Register(new FakeProvider("A", ProcessImpact.Light, "a1.csv", "a2.csv"))
            .Register(new FakeProvider("B", ProcessImpact.Light, "b1.csv"));

        var composite = new CompositeFileValueProvider("C", connectors, "A", "B");

        var collected = new List<string>();
        await foreach (var fv in composite.ProvideAsync())
            collected.Add(fv.Name);

        Assert.Equal(["a1.csv", "a2.csv", "b1.csv"], collected);
    }

    [Fact]
    public void Provider_PerformanceImpact_IsMaxOfUnderlying()
    {
        var connectors = new FileValueConnectors();
        connectors
            .Register(new FakeProvider("A", ProcessImpact.Light))
            .Register(new FakeProvider("B", ProcessImpact.Heavy));

        var composite = new CompositeFileValueProvider("C", connectors, "A", "B");

        Assert.Equal(ProcessImpact.Heavy, composite.PerformanceImpact);
    }

    [Fact]
    public void Provider_FileReferences_PointToUnderlyingProviderCodes()
    {
        var connectors = new FileValueConnectors();
        connectors
            .Register(new FakeProvider("A", ProcessImpact.Light, "a1.csv"))
            .Register(new FakeProvider("B", ProcessImpact.Light, "b1.csv"));

        var composite = new CompositeFileValueProvider("C", connectors, "A", "B");

        var refs = new List<FileReference>();
        composite.Provide(null, (_, fr) => refs.Add(fr), CancellationToken.None);

        Assert.Equal("A", refs[0].Connector);
        Assert.Equal("B", refs[1].Connector);
    }

    [Fact]
    public void Provider_Provide_StopsOnCancellation()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var connectors = new FileValueConnectors();
        connectors
            .Register(new FakeProvider("A", ProcessImpact.Light, "a1.csv"))
            .Register(new FakeProvider("B", ProcessImpact.Light, "b1.csv"));

        var composite = new CompositeFileValueProvider("C", connectors, "A", "B");

        var collected = new List<string>();
        composite.Provide(null, (fv, _) => collected.Add(fv.Name), cts.Token);

        Assert.Empty(collected);
    }

    // --- Lazy resolution: underlying providers registered after composite is created ---

    [Fact]
    public void Provider_ResolvesProvidersLazily()
    {
        var connectors = new FileValueConnectors();
        var composite = new CompositeFileValueProvider("C", connectors, "A", "B");

        connectors
            .Register(new FakeProvider("A", ProcessImpact.Light, "a1.csv"))
            .Register(new FakeProvider("B", ProcessImpact.Light, "b1.csv"));

        var collected = new List<string>();
        composite.Provide(null, (fv, _) => collected.Add(fv.Name), CancellationToken.None);

        Assert.Equal(["a1.csv", "b1.csv"], collected);
    }
}
