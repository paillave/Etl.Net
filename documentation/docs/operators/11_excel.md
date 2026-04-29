---
sidebar_position: 11
---

# Excel files

`Paillave.Etl.ExcelFile` consumes and produces `.xlsx` workbooks
through [EPPlus](https://github.com/EPPlusSoftware/EPPlus). The package
exposes two families of operators:

| Family | Operators |
| --- | --- |
| Read | `CrossApplyExcelSheets`, `CrossApplyExcelDataTables`, `CrossApplyExcelDatasets`, `CrossApplyExcelRows` |
| Write | `ToExcelFile` |

> All snippets below are exercised by
> `src/Paillave.Etl.Tests/DocExamples/ExcelOperatorsExamplesTests.cs`.
>
> :::info Linux / CI requirement
> EPPlus uses `System.Drawing` for column auto-fitting. On Linux you
> must install **`libgdiplus`** (`sudo apt-get install libgdiplus`).
> :::

## Defining the layout

`ExcelFileDefinition<T>` describes a sheet: where the header is, where
the dataset starts, the orientation, and how columns map to properties.
It is built by an `ExcelFileArgBuilder`:

```cs
b => b.UseType<CountryRow>()              // map by property name
      .HasColumnHeader("A1:C1")            // header row range
      .WithDataset("A2:C100");             // dataset range
```

| Builder method | Purpose |
| --- | --- |
| `UseType<T>()` | Default mapping — property name = column header |
| `UseMap<T>(i => new T { ... i.ToColumn<...>(name &#124; index) ... })` | Explicit fluent mapping |
| `HasColumnHeader("A1:C1")` | Range of the header row (omit for headerless sheets) |
| `WithDataset("A2:C100")` | Range that contains the data |
| `WithVerticalDataset()` / `WithHorizontalDataset()` | Pivot the orientation |
| `WithCultureInfo("fr-FR")` | Parse numbers / dates with a culture |
| `RespectHeaderCase()` | Match column names case-sensitively |

The `IFieldMapper` exposed by `UseMap` supports both **column name**
(`i.ToColumn<int>("Population")`) and **column index**
(`i.ToColumn<int>(3)` — 1-based) lookups, plus the helpers documented
in the [text-file page](./3_textFile.md).

## Reading a sheet with a header

> Test: `CrossApplyExcelRows_WithHeader_ReadsTypedRows`

```cs {3-6}
root
    .CrossApply("source", _ => new[] { fileValue })
    .CrossApplyExcelSheets("sheets")
    .CrossApplyExcelRows("rows", b => b.UseType<CountryRow>()
        .HasColumnHeader("A1:C1")
        .WithDataset("A2:C100"))
    .Do("log", r => Console.WriteLine($"{r.Code} = {r.Name}"));
```

`CrossApplyExcelSheets` opens the workbook and emits one
`ExcelSheetSelection` per worksheet. `CrossApplyExcelRows` then reads
the sheet using the `ExcelFileDefinition` and emits typed rows.

## Reading a headerless sheet (positional)

> Test: `CrossApplyExcelRows_NoHeader_UsesPositionalMapping`

```cs {2-7}
.CrossApplyExcelRows("rows", b => b.UseMap<CountryRow>(i => new CountryRow
{
    Code       = i.ToColumn<string>(1),
    Name       = i.ToColumn<string>(2),
    Population = i.ToColumn<int>(3),
}).WithDataset("A1:C100"))
```

When the workbook has no header row, drop `HasColumnHeader(...)` and
address columns by 1-based index inside the explicit mapping.

## Filtering sheets

> Test: `CrossApplyExcelSheets_FiltersByName`

```cs
.CrossApplyExcelSheets("sheets")
.Where("only countries", s => s.Name == "Countries")
.CrossApplyExcelRows("rows", b => ...);
```

`ExcelSheetSelection` exposes the worksheet `Name`, so any
[`Where`](./1_core.md#filtering-and-decision) clause can be used to
keep only the sheets you care about.

## Lower-level readers

| Operator | Emits | Use case |
| --- | --- | --- |
| `CrossApplyExcelSheets` | `ExcelSheetSelection` | Filter / dispatch by sheet name before parsing |
| `CrossApplyExcelDataTables` | `System.Data.DataTable` | Bulk read entire sheets into ADO.NET tables |
| `CrossApplyExcelDatasets` | Custom `T` from a `DataTable` | When you need to walk the raw cell grid |
| `CrossApplyExcelRows` | Typed `T` | Recommended path — strongly-typed rows |

## Writing an Excel file

> Test: `ToExcelFile_WritesRows_RoundTrip`

```cs {3-6}
root
    .CrossApply("rows", r => incomingRows)
    .ToExcelFile("write", "out.xlsx", b => b.UseType<CountryRow>()
        .HasColumnHeader("A1:C1")
        .WithDataset("A2:C100"))
    .Do("publish", file => upload(file));
```

`ToExcelFile` materialises the entire upstream stream into memory,
writes a workbook with a single sheet, and emits an `IFileValue`
named after the second argument. The same `ExcelFileDefinition` API
that drives the reader also drives the writer — including
`WithCultureInfo` for numeric/date formatting.

## Cheat sheet

| Intent | Snippet |
| --- | --- |
| Read every sheet, every row | `.CrossApplyExcelSheets(n).CrossApplyExcelRows(n2, b => b.UseType<T>().HasColumnHeader("A1:C1").WithDataset("A2:C100"))` |
| Read only specific sheets | `.CrossApplyExcelSheets(n).Where(n2, s => s.Name == "Foo")...` |
| No header — positional mapping | `b.UseMap<T>(i => new T { X = i.ToColumn<int>(1) }).WithDataset("A1:C100")` |
| Localised parsing | `b.UseType<T>().WithCultureInfo("fr-FR")...` |
| Read into ADO.NET DataTable | `.CrossApplyExcelDataTables(n)` |
| Write a workbook | `.ToExcelFile(n, "out.xlsx", b => b.UseType<T>().HasColumnHeader("A1:C1").WithDataset("A2:C100"))` |
