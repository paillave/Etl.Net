using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.TextFile;
using Xunit;

namespace Paillave.Etl.Tests.DocExamples;

/// <summary>
/// Each test in this file is a runnable mirror of an example from
/// <c>documentation/docs/operators/3_textFile.md</c>. They all parse
/// (or write) flat-text files in memory so they can run in the test
/// harness without touching the file system.
/// </summary>
public class TextFileOperatorsExamplesTests
{
    // ===================================================================
    // Domain rows
    // ===================================================================

    public class TradeRow
    {
        public string Symbol { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime TradeDate { get; set; }
    }

    public class FixedRow
    {
        public string Code { get; set; } = "";
        public string Label { get; set; } = "";
        public int Quantity { get; set; }
    }

    // ===================================================================
    // In-memory IFileValue used to feed the parsing operators
    // ===================================================================

    public sealed class InMemoryFileValue : FileValueBase
    {
        private readonly byte[] _bytes;
        public override string Name { get; }

        public InMemoryFileValue(string name, string content, Encoding? encoding = null)
        {
            _bytes = (encoding ?? Encoding.UTF8).GetBytes(content);
            Name = name;
        }

        protected override void DeleteFile() { }
        public override Stream GetContent() => new MemoryStream(_bytes, writable: false);
        public override StreamWithResource OpenContent()
            => new StreamWithResource(new MemoryStream(_bytes, writable: false));
    }

    // ===================================================================
    // CrossApplyTextFile — parse a CSV with a header row
    // ===================================================================

    [Fact]
    public async Task CrossApplyTextFile_ParsesCsvWithHeader()
    {
        const string csv =
            "Symbol;Quantity;Price;TradeDate\n" +
            "AAPL;100;150.25;2024-01-15\n" +
            "MSFT;50;310.50;2024-01-16\n";

        var collected = new ConcurrentBag<TradeRow>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            new InMemoryFileValue("trades.csv", csv),
            root => root
                .CrossApply("emit file", file => new[] { file })
                .CrossApplyTextFile("parse", FlatFileDefinition.Create(i => new TradeRow
                {
                    Symbol    = i.ToColumn("Symbol"),
                    Quantity  = i.ToNumberColumn<int>("Quantity", "."),
                    Price     = i.ToNumberColumn<decimal>("Price", "."),
                    TradeDate = i.ToDateColumn("TradeDate", "yyyy-MM-dd"),
                }).IsColumnSeparated(';'))
                .Do("collect", collected.Add));

        Assert.False(status.Failed);
        Assert.Equal(2, collected.Count);
        Assert.Contains(collected, r => r.Symbol == "AAPL" && r.Quantity == 100 && r.Price == 150.25m);
        Assert.Contains(collected, r => r.Symbol == "MSFT" && r.Quantity == 50);
    }

    // ===================================================================
    // Positional columns — no header, indices instead of names
    // ===================================================================

    [Fact]
    public async Task CrossApplyTextFile_ParsesPositionalCsv()
    {
        const string csv =
            "AAPL,100,150.25\n" +
            "MSFT,50,310.50\n";

        var collected = new ConcurrentBag<TradeRow>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            new InMemoryFileValue("trades.csv", csv),
            root => root
                .CrossApply("emit file", file => new[] { file })
                .CrossApplyTextFile("parse", FlatFileDefinition.Create(i => new TradeRow
                {
                    Symbol   = i.ToColumn(0),
                    Quantity = i.ToNumberColumn<int>(1, "."),
                    Price    = i.ToNumberColumn<decimal>(2, "."),
                }).IsColumnSeparated(','))
                .Do("collect", collected.Add));

        Assert.False(status.Failed);
        Assert.Equal(2, collected.Count);
        Assert.Contains(collected, r => r.Symbol == "AAPL" && r.Price == 150.25m);
    }

    // ===================================================================
    // Fixed-width columns — slice by character size
    // ===================================================================

    [Fact]
    public async Task CrossApplyTextFile_ParsesFixedWidth()
    {
        // Code: 4 chars | Label: 10 chars | Quantity: 5 chars
        const string fixedFile =
            "AAA Apple     00100\n" +
            "BBB Banana    00250\n";

        var collected = new ConcurrentBag<FixedRow>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            new InMemoryFileValue("ref.txt", fixedFile),
            root => root
                .CrossApply("emit file", file => new[] { file })
                .CrossApplyTextFile("parse", FlatFileDefinition.Create(i => new FixedRow
                {
                    Code     = i.ToColumn(0, 4),
                    Label    = i.ToColumn(1, 10),
                    Quantity = i.ToNumberColumn<int>(2, 5, "."),
                }))
                .Do("collect", c => collected.Add(c)));

        Assert.False(status.Failed);
        Assert.Equal(2, collected.Count);
        Assert.Contains(collected, r => r.Code.Trim() == "AAA" && r.Label.Trim() == "Apple" && r.Quantity == 100);
        Assert.Contains(collected, r => r.Code.Trim() == "BBB" && r.Quantity == 250);
    }

    // ===================================================================
    // Culture-aware parsing — French decimals
    // ===================================================================

    [Fact]
    public async Task CrossApplyTextFile_ParsesWithCustomCulture()
    {
        const string csv =
            "Symbol;Price\n" +
            "EUR;1 234,50\n";

        var collected = new ConcurrentBag<(string, decimal)>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            new InMemoryFileValue("fx.csv", csv),
            root => root
                .CrossApply("emit file", file => new[] { file })
                .CrossApplyTextFile("parse", FlatFileDefinition.Create(i => new
                {
                    Symbol = i.ToColumn("Symbol"),
                    Price  = i.ToNumberColumn<decimal>("Price", ",", " "),
                }).IsColumnSeparated(';').WithCultureInfo(CultureInfo.GetCultureInfo("fr-FR")))
                .Do("collect", row => collected.Add((row.Symbol, row.Price))));

        Assert.False(status.Failed);
        Assert.Single(collected);
        Assert.Equal(("EUR", 1234.50m), collected.Single());
    }

    // ===================================================================
    // Stream metadata — source name, line number, row guid
    // ===================================================================

    [Fact]
    public async Task CrossApplyTextFile_ExposesMetadataColumns()
    {
        const string csv =
            "Symbol;Quantity\n" +
            "AAPL;100\n" +
            "MSFT;50\n";

        var collected = new ConcurrentBag<(string Source, int Line, string Symbol)>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            new InMemoryFileValue("trades.csv", csv),
            root => root
                .CrossApply("emit file", file => new[] { file })
                .CrossApplyTextFile("parse", FlatFileDefinition.Create(i => new
                {
                    Symbol   = i.ToColumn("Symbol"),
                    Quantity = i.ToNumberColumn<int>("Quantity", "."),
                    SourceName = i.ToSourceName(),
                    LineNumber = i.ToLineNumber(),
                }).IsColumnSeparated(';'))
                .Do("collect", row => collected.Add((row.SourceName, row.LineNumber, row.Symbol))));

        Assert.False(status.Failed);
        Assert.Equal(2, collected.Count);
        Assert.All(collected, t => Assert.Equal("trades.csv", t.Source));
        Assert.Contains(collected, t => t.Symbol == "AAPL");
        // Line numbers count data rows (1-based) starting after the header.
        Assert.Equal(new[] { 1, 2 }, collected.Select(t => t.Line).OrderBy(i => i));
    }

    // ===================================================================
    // ToTextFileValue — write the stream out as a CSV
    // ===================================================================

    [Fact]
    public async Task ToTextFileValue_WritesCsvWithHeader()
    {
        var captured = new List<IFileValue>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            "go",
            root => root
                .CrossApply("rows", _ => new[]
                {
                    new TradeRow { Symbol = "AAPL", Quantity = 10, Price = 150.25m, TradeDate = new DateTime(2024,1,15) },
                    new TradeRow { Symbol = "MSFT", Quantity = 20, Price = 310.50m, TradeDate = new DateTime(2024,1,16) },
                })
                .ToTextFileValue("write", "out.csv", FlatFileDefinition.Create(i => new TradeRow
                {
                    Symbol    = i.ToColumn("Symbol"),
                    Quantity  = i.ToNumberColumn<int>("Quantity", "."),
                    Price     = i.ToNumberColumn<decimal>("Price", "."),
                    TradeDate = i.ToDateColumn("TradeDate", "yyyy-MM-dd"),
                }).IsColumnSeparated(';'))
                .Do("capture", file => captured.Add(file)));

        Assert.False(status.Failed);
        Assert.Single(captured);

        using var reader = new StreamReader(captured[0].GetContent());
        var content = reader.ReadToEnd();
        Assert.Contains("Symbol;Quantity;Price;TradeDate", content);
        Assert.Contains("AAPL;10;150.25;2024-01-15", content);
        Assert.Contains("MSFT;20;310.50;2024-01-16", content);
    }
}
