using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml;
using Paillave.Etl.Core;
using Paillave.Etl.ExcelFile;
using Paillave.Etl.Reactive.Operators;
using Xunit;

namespace Paillave.Etl.Tests.DocExamples;

// Examples mirrored by `documentation/docs/operators/11_excel.md`.
// All scenarios produce / consume an .xlsx in memory through EPPlus.
public class ExcelOperatorsExamplesTests
{
    public sealed class CountryRow
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int Population { get; set; }
    }

    private static IFileValue BuildXlsx(string fileName, Action<ExcelWorksheet> writeSheet, string sheetName = "Countries")
    {
        var ms = new MemoryStream();
        using (var pkg = new ExcelPackage(ms))
        {
            var ws = pkg.Workbook.Worksheets.Add(sheetName);
            writeSheet(ws);
            pkg.Save();
        }
        ms.Seek(0, SeekOrigin.Begin);
        return new InMemoryFileValue(ms, fileName);
    }

    // ===================================================================
    // Read rows from a sheet that has a header row
    // ===================================================================

    [Fact]
    public async Task CrossApplyExcelRows_WithHeader_ReadsTypedRows()
    {
        var xlsx = BuildXlsx("countries.xlsx", ws =>
        {
            ws.Cells["A1"].Value = "Code";
            ws.Cells["B1"].Value = "Name";
            ws.Cells["C1"].Value = "Population";
            ws.Cells["A2"].Value = "FR"; ws.Cells["B2"].Value = "France";  ws.Cells["C2"].Value = 67;
            ws.Cells["A3"].Value = "DE"; ws.Cells["B3"].Value = "Germany"; ws.Cells["C3"].Value = 83;
        });

        var rows = new ConcurrentBag<CountryRow>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            xlsx,
            root => root
                .CrossApply("source", _ => new[] { xlsx })
                .CrossApplyExcelSheets("sheets")
                .CrossApplyExcelRows("rows", b => b.UseType<CountryRow>()
                    .HasColumnHeader("A1:C1")
                    .WithDataset("A2:C100"))
                .Do("collect", r => rows.Add(r)));

        Assert.False(status.Failed);
        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, r => r.Code == "FR" && r.Name == "France"  && r.Population == 67);
        Assert.Contains(rows, r => r.Code == "DE" && r.Name == "Germany" && r.Population == 83);
    }

    // ===================================================================
    // Read rows from a sheet without a header (positional mapping)
    // ===================================================================

    [Fact]
    public async Task CrossApplyExcelRows_NoHeader_UsesPositionalMapping()
    {
        var xlsx = BuildXlsx("countries.xlsx", ws =>
        {
            ws.Cells["A1"].Value = "FR"; ws.Cells["B1"].Value = "France";  ws.Cells["C1"].Value = 67;
            ws.Cells["A2"].Value = "IT"; ws.Cells["B2"].Value = "Italy";   ws.Cells["C2"].Value = 60;
        });

        var rows = new ConcurrentBag<CountryRow>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            xlsx,
            root => root
                .CrossApply("source", _ => new[] { xlsx })
                .CrossApplyExcelSheets("sheets")
                .CrossApplyExcelRows("rows", b => b.UseMap<CountryRow>(i => new CountryRow
                {
                    Code       = i.ToColumn<string>(1),
                    Name       = i.ToColumn<string>(2),
                    Population = i.ToColumn<int>(3),
                }).WithDataset("A1:C100"))
                .Do("collect", r => rows.Add(r)));

        Assert.False(status.Failed);
        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, r => r.Code == "FR" && r.Name == "France"  && r.Population == 67);
        Assert.Contains(rows, r => r.Code == "IT" && r.Name == "Italy"   && r.Population == 60);
    }

    // ===================================================================
    // Filter sheets by name (multiple sheets in workbook)
    // ===================================================================

    [Fact]
    public async Task CrossApplyExcelSheets_FiltersByName()
    {
        var xlsx = BuildXlsx("multi.xlsx", ws =>
        {
            ws.Cells["A1"].Value = "Code"; ws.Cells["B1"].Value = "Name";
            ws.Cells["A2"].Value = "FR"; ws.Cells["B2"].Value = "France";
        }, sheetName: "Countries");

        // patch in a second sheet
        IFileValue twoSheets;
        {
            var src = xlsx.GetContent(); src.Position = 0;
            var ms = new MemoryStream(); src.CopyTo(ms); ms.Position = 0;
            using (var pkg = new ExcelPackage(ms))
            {
                var ignored = pkg.Workbook.Worksheets.Add("Garbage");
                ignored.Cells["A1"].Value = "noise";
                pkg.Save();
            }
            ms.Position = 0;
            twoSheets = new InMemoryFileValue(ms, "multi.xlsx");
        }

        var sheetNames = new ConcurrentBag<string>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            twoSheets,
            root => root
                .CrossApply("source", _ => new[] { twoSheets })
                .CrossApplyExcelSheets("sheets")
                .Where("only countries", s => s.Name == "Countries")
                .Do("collect", s => sheetNames.Add(s.Name)));

        Assert.False(status.Failed);
        Assert.Equal(new[] { "Countries" }, sheetNames);
    }

    // ===================================================================
    // Round-trip: write rows → read them back
    // ===================================================================

    [Fact]
    public async Task ToExcelFile_WritesRows_RoundTrip()
    {
        var rows = new[]
        {
            new CountryRow { Code = "FR", Name = "France",  Population = 67 },
            new CountryRow { Code = "DE", Name = "Germany", Population = 83 },
        };

        IFileValue? produced = null;

        // 1. write
        var write = await StreamProcessRunner.CreateAndExecuteAsync(
            rows,
            root => root
                .CrossApply("rows", r => r)
                .ToExcelFile("write", "out.xlsx", b => b.UseType<CountryRow>()
                    .HasColumnHeader("A1:C1")
                    .WithDataset("A2:C100"))
                .Do("capture", f => produced = f));

        Assert.False(write.Failed);
        Assert.NotNull(produced);
        Assert.Equal("out.xlsx", produced!.Name);

        // 2. read back
        var readBack = new ConcurrentBag<CountryRow>();
        var read = await StreamProcessRunner.CreateAndExecuteAsync(
            produced,
            root => root
                .CrossApply("source", _ => new[] { produced! })
                .CrossApplyExcelSheets("sheets")
                .CrossApplyExcelRows("rows", b => b.UseType<CountryRow>()
                    .HasColumnHeader("A1:C1")
                    .WithDataset("A2:C100"))
                .Do("collect", r => readBack.Add(r)));

        Assert.False(read.Failed);
        Assert.Equal(2, readBack.Count);
        Assert.Contains(readBack, r => r.Code == "FR" && r.Population == 67);
        Assert.Contains(readBack, r => r.Code == "DE" && r.Population == 83);
    }
}
