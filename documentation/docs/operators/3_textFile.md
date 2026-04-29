---
sidebar_position: 3
---

# Text files

The `Paillave.Etl.TextFile` package parses and produces flat-text
files — comma- or semicolon-separated values, tab-delimited files,
fixed-width files. Two operators do all the work:

| Operator | Purpose |
| --- | --- |
| `CrossApplyTextFile` | Read every line of an `IFileValue`, parse it into a strongly-typed object and push it downstream. |
| `ToTextFileValue` | Buffer a stream and emit a single text `IFileValue` (a CSV, TSV, or fixed-width file). |

> All snippets below are mirrored by tests in
> `src/Paillave.Etl.Tests/DocExamples/TextFileOperatorsExamplesTests.cs`.
> Each section names the matching test for traceability.

## Defining the file shape

The shape of every text file is described by a `FlatFileDefinition<T>`,
built with a single lambda that maps each property of the destination
type to a column:

```cs
var def = FlatFileDefinition.Create(i => new TradeRow
{
    Symbol    = i.ToColumn("Symbol"),
    Quantity  = i.ToNumberColumn<int>("Quantity", "."),
    Price     = i.ToNumberColumn<decimal>("Price", "."),
    TradeDate = i.ToDateColumn("TradeDate", "yyyy-MM-dd"),
})
.IsColumnSeparated(';');
```

The `i` parameter exposes the `IFieldMapper` interface. Its main
helpers are:

| Method | Use |
| --- | --- |
| `ToColumn(name)` / `ToColumn(index)` | String column |
| `ToColumn<T>(name)` / `ToColumn<T>(index)` | Generic conversion (`int`, `Guid`, `bool`, …) |
| `ToNumberColumn<T>(name, decimalSep, groupSep?)` | Numeric column with explicit separators |
| `ToDateColumn(name, format)` | Date column with a `DateTime.ParseExact` format |
| `ToCulturedDateColumn(name, cultureName, format?)` | Date parsed with a specific culture |
| `ToOptionalDateColumn` / `ToOptional...` | Same, but produces `null` on empty input |
| `ToBooleanColumn(name, trueValue, falseValue)` | `true`/`false` from custom tokens |
| `ToColumn(index, size)` | **Fixed-width** column — the size in characters |
| `ToSourceName()` | Inject the file name into a property |
| `ToLineNumber()` | Inject the data-row index (1-based, skips the header) |
| `ToRowGuid()` | Inject a freshly generated `Guid` per row |
| `Ignore<T>()` | Place-holder for a property that should not be read |

`FlatFileDefinition<T>` itself exposes the file-level switches:

| Method | Effect |
| --- | --- |
| `IsColumnSeparated(sep, textDelimiter?)` | Default. CSV/TSV with a separator (default `;`) and an optional text-delimiter (default `"`). |
| `HasFixedColumnWidth(int...)` | Switches to the fixed-width parser. Sizes are inferred from `ToColumn(index, size)` calls if any. |
| `IgnoreFirstLines(n)` | Skip `n` lines before reading the header (for files prefixed with banners or comments). |
| `WithLinePreProcessor(Func<string,string>)` | Transform every raw line before it is split (e.g. trim BOMs, replace separators). |
| `WithValuePreProcessor("Property", Func<string,string>)` | Transform a single column's text before conversion. |
| `RespectHeaderCase(bool)` | By default headers match case-insensitively. |
| `WithCultureInfo(culture)` / `WithCultureInfo("fr-FR")` | Default culture for number/date conversions. |
| `WithEncoding(encoding)` | Encoding used by `ToTextFileValue`. |

## Reading a file

### Header-based CSV

> Test: `CrossApplyTextFile_ParsesCsvWithHeader`

```cs {3-9}
fileStream
    .CrossApplyTextFile("parse trades", FlatFileDefinition.Create(i => new TradeRow
    {
        Symbol    = i.ToColumn("Symbol"),
        Quantity  = i.ToNumberColumn<int>("Quantity", "."),
        Price     = i.ToNumberColumn<decimal>("Price", "."),
        TradeDate = i.ToDateColumn("TradeDate", "yyyy-MM-dd"),
    }).IsColumnSeparated(';'))
    .Do("log", t => Console.WriteLine(t.Symbol));
```

Input:

```text
Symbol;Quantity;Price;TradeDate
AAPL;100;150.25;2024-01-15
MSFT;50;310.50;2024-01-16
```

Output: two `TradeRow` instances flowing through the stream.

The mapper is **typed**: `Quantity = i.ToNumberColumn<int>(…)` produces
an `int` and any line where the field cannot be parsed throws a
`FlatFileFieldDeserializeException` whose `LineNumber` and `ColumnName`
properties point right at the bad cell.

### Positional CSV (no header)

> Test: `CrossApplyTextFile_ParsesPositionalCsv`

```cs {3-7}
fileStream
    .CrossApplyTextFile("parse", FlatFileDefinition.Create(i => new TradeRow
    {
        Symbol   = i.ToColumn(0),
        Quantity = i.ToNumberColumn<int>(1, "."),
        Price    = i.ToNumberColumn<decimal>(2, "."),
    }).IsColumnSeparated(','));
```

Pass column **indices** (0-based) instead of names. The definition
becomes header-less automatically as soon as no `ColumnName` is
provided.

### Fixed-width files

> Test: `CrossApplyTextFile_ParsesFixedWidth`

```cs {3-7}
fileStream
    .CrossApplyTextFile("parse", FlatFileDefinition.Create(i => new FixedRow
    {
        Code     = i.ToColumn(0, 4),     // 4 chars
        Label    = i.ToColumn(1, 10),    // 10 chars
        Quantity = i.ToNumberColumn<int>(2, 5, "."), // 5 chars
    }));
```

Adding a `size` argument to every column is enough — the definition
auto-switches to `HasFixedColumnWidth(...)` with the column sizes you
just declared. Strings are returned space-padded; trim them in the
selector if needed.

### Culture-aware numbers and dates

> Test: `CrossApplyTextFile_ParsesWithCustomCulture`

```cs
.IsColumnSeparated(';')
.WithCultureInfo(CultureInfo.GetCultureInfo("fr-FR"));
```

```cs
Price = i.ToNumberColumn<decimal>("Price", ",", " "),
```

Setting `WithCultureInfo("fr-FR")` makes `1 234,50` parse as
`1234.5m`. Per-column overrides via
`ToCulturedDateColumn(name, "en-US", "MM/dd/yyyy")` are also
supported.

### Stream-level metadata

> Test: `CrossApplyTextFile_ExposesMetadataColumns`

```cs {5-6}
FlatFileDefinition.Create(i => new
{
    Symbol     = i.ToColumn("Symbol"),
    Quantity   = i.ToNumberColumn<int>("Quantity", "."),
    SourceName = i.ToSourceName(),    // "trades.csv"
    LineNumber = i.ToLineNumber(),    // 1, 2, 3, …
});
```

`ToSourceName()` injects `IFileValue.Name` into a property,
`ToLineNumber()` injects the row index, and `ToRowGuid()` generates a
fresh `Guid` per row. They are invaluable for traceability when many
files merge into a single downstream stream.

### Pre-processing dirty input

```cs
.WithLinePreProcessor(line => line.Replace('\t', ';'))
.WithValuePreProcessor("Symbol", v => v.Trim().ToUpperInvariant());
```

`WithLinePreProcessor` runs once per raw line (after the file is
split), `WithValuePreProcessor` runs once per cell. Combine both to
fix a malformed file without writing a custom parser.

### Skipping prologue lines

```cs
FlatFileDefinition.Create(...)
    .IgnoreFirstLines(3);   // banner + blank + comment
```

`IgnoreFirstLines(n)` drops the first *n* lines — header detection
runs *after* this step.

## Writing a file

### `ToTextFileValue` — emit a CSV

> Test: `ToTextFileValue_WritesCsvWithHeader`

```cs {2-8}
trades
    .ToTextFileValue("write trades", "out.csv", FlatFileDefinition.Create(i => new TradeRow
    {
        Symbol    = i.ToColumn("Symbol"),
        Quantity  = i.ToNumberColumn<int>("Quantity", "."),
        Price     = i.ToNumberColumn<decimal>("Price", "."),
        TradeDate = i.ToDateColumn("TradeDate", "yyyy-MM-dd"),
    }).IsColumnSeparated(';'));
```

Output:

```text
Symbol;Quantity;Price;TradeDate
AAPL;10;150.25;2024-01-15
MSFT;20;310.50;2024-01-16
```

`ToTextFileValue` returns an `ISingleStream<IFileValue>`. Combine with
`Subscribe` connectors (file system, FTP, SFTP, S3, Azure Storage…) to
deliver the file to its destination, or call `.Get()` on the value to
read its content.

The `Correlated<T>` overload preserves stream correlation tokens —
useful when partitioning a stream by group and emitting one file per
partition.

### Custom culture & encoding for output

```cs
.IsColumnSeparated(';')
.WithCultureInfo("fr-FR")
.WithEncoding(Encoding.UTF8);
```

Numbers are formatted using the file definition's culture; the same
`ToNumberColumn(... separators ...)` declaration also drives the
output formatter so round-trips are loss-less.

### Routing to multiple destinations

The `destinations` argument of `ToTextFileValue` accepts a
`Dictionary<string, IEnumerable<Destination>>` describing connector
names and folder paths. See [Recipes — dealing with files](../recipes/1_dealWithFiles.md)
for the full list.

## Cheat sheet

| Intent | Snippet |
| --- | --- |
| Read CSV with header | `CrossApplyTextFile(name, FlatFileDefinition.Create(...).IsColumnSeparated(';'))` |
| Read TSV | `.IsColumnSeparated('\t')` |
| Read positional file | use `ToColumn(index)` instead of `ToColumn("Name")` |
| Read fixed-width file | add `size` to every column: `ToColumn(idx, size)` |
| Skip prologue lines | `.IgnoreFirstLines(n)` |
| Custom culture | `.WithCultureInfo("fr-FR")` |
| Inject metadata | `i.ToSourceName()`, `i.ToLineNumber()`, `i.ToRowGuid()` |
| Sanitize lines / values | `.WithLinePreProcessor(...)`, `.WithValuePreProcessor("X", ...)` |
| Write CSV | `.ToTextFileValue(name, "out.csv", FlatFileDefinition.Create(...).IsColumnSeparated(';'))` |
