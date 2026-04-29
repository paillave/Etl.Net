---
sidebar_position: 5
---

# SQL Server / ADO.NET

`Paillave.Etl.SqlServer` exposes three operators that talk to a
relational database through a plain
[`IDbConnection`](https://learn.microsoft.com/dotnet/api/system.data.idbconnection)
resolved from the DI container:

| Operator | Purpose |
| --- | --- |
| `CrossApplySqlServerQuery` | Run a parameterised `SELECT` and stream the rows. |
| `ToSqlCommand` | Run a parameterised `INSERT` / `UPDATE` / `DELETE` for every input row. |
| `SqlServerSave` | High-throughput upsert that builds a SQL Server-specific `MERGE` statement. |

> Despite the namespace name, only `SqlServerSave` is SQL Server-specific.
> `CrossApplySqlServerQuery` and `ToSqlCommand` use the abstract
> `IDbConnection` interface and run against any ADO.NET provider —
> SQL Server, PostgreSQL, MySQL, SQLite, Oracle…

> All snippets below are mirrored by tests in
> `src/Paillave.Etl.Tests/DocExamples/SqlOperatorsExamplesTests.cs`,
> which run against an in-memory SQLite database.

## Wiring a connection

ETL.Net resolves an `IDbConnection` from `ExecutionOptions.Services`.
Open it inside the factory; the operators reuse the open connection
during one execution scope.

```cs
var services = new ServiceCollection()
    .AddTransient<IDbConnection>(_ =>
    {
        var c = new SqlConnection("Server=...;Database=Demo;Integrated Security=true;");
        c.Open();
        return c;
    })
    .BuildServiceProvider();

await StreamProcessRunner.CreateAndExecuteAsync(
    config,
    job,
    new ExecutionOptions<TConfig> { Services = services });
```

For multi-tenant or sharded scenarios, the API exposes
`.WithKeyedConnection("primary")` on each operator.
:::caution Keyed connection limitation
The runner currently wraps the user-provided service provider in a
`CompositeServiceProvider` that does not forward keyed lookups. Until
that wrapper is upgraded, `WithKeyedConnection` should be considered
experimental — prefer one provider per tenant for now.
:::

## `CrossApplySqlServerQuery` — read rows

### Default mapping (column name → property name)

> Test: `CrossApplySqlServerQuery_DefaultMapping_MatchesByPropertyName`

```cs {3-5}
root
    .CrossApply("trigger", _ => new[] { new { Code = "DE" } })
    .CrossApplySqlServerQuery("read",
        o => o.FromQuery("SELECT Code, Name FROM Country WHERE Code = @Code")
              .WithMapping<CountryRow>())
    .Do("log", c => Console.WriteLine($"{c.Code} = {c.Name}"));
```

The `@Code` parameter is filled in from the property of the same
name on the upstream row. `WithMapping<TOut>()` tells the operator to
build `CountryRow` instances by matching column names to property
names case-insensitively.

### Explicit mapping

> Test: `CrossApplySqlServerQuery_ReadsRowsAndMapsToType`

```cs {4-9}
root
    .CrossApply("trigger", _ => new[] { new { Filter = "F" } })
    .CrossApplySqlServerQuery("read",
        o => o.FromQuery("SELECT Code, Name FROM Country WHERE Code LIKE @Filter || '%'")
              .WithMapping<CountryRow>(i => new CountryRow
              {
                  Code = i.ToColumn<string>("Code"),
                  Name = i.ToColumn<string>("Name"),
              }))
    .Do("log", c => Console.WriteLine(c.Code));
```

The mapping lambda exposes an `ISqlResultMapper`:

| Method | Use |
| --- | --- |
| `ToColumn<T>(name)` / `ToColumn(name)` | Generic column |
| `ToNumberColumn<T>(name)` | Force numeric conversion |
| `ToDateColumn(name)` | Required `DateTime` |
| `ToOptionalDateColumn(name)` | Nullable `DateTime?` |
| `ToBooleanColumn(name)` | Required `bool` |
| `ToOptionalBooleanColumn(name)` | Nullable `bool?` |
| Same methods without arguments | Match by *property* name (default mapping) |

### Parameter binding

The operator scans the SQL for `@param` tokens (case-insensitive) and
binds each one to the property of the upstream row that has the same
name. `null` values are sent as `DBNull.Value`. **No string concatenation**
is performed — every value is a real SQL parameter, immune to SQL
injection.

## `ToSqlCommand` — execute a statement per row

> Test: `ToSqlCommand_ExecutesStatementPerRow`

```cs {3-4}
root
    .CrossApply("rows", _ => incomingCountries)
    .ToSqlCommand("insert",
        "INSERT INTO Country(Code, Name) VALUES(@Code, @Name) RETURNING Code, Name");
```

For every upstream row, `ToSqlCommand` runs the statement with the
row's properties as parameters. Use it for bespoke `INSERT`,
`UPDATE`, `DELETE`, or stored-procedure calls.

If the statement contains a `RETURNING` / `OUTPUT` clause that
returns columns of the same name as the row's properties, those
properties are **rewritten in place** with the database values —
typical for round-tripping freshly-generated identity keys.

The `Correlated<T>` overload preserves correlation tokens, so this
operator can sit inside a partitioned sub-stream without breaking
ordering.

## `SqlServerSave` — bulk upsert (SQL Server only)

```cs
root
    .CrossApply("rows", _ => trades)
    .SqlServerSave("save trades", o => o
        .ToTable("dbo.Trade")
        .PivotOn(t => new { t.TradeId })   // business key — used to detect existing rows
        .ComputedColumns(t => new { t.RowVersion })); // columns the DB writes itself
```

`SqlServerSave` builds a single SQL statement of the form

```sql
IF EXISTS (SELECT 1 FROM dbo.Trade AS p WHERE p.[TradeId] = @TradeId)
    UPDATE p SET [Amount] = @Amount, ... OUTPUT inserted.* FROM dbo.Trade AS p WHERE ...;
ELSE
    INSERT INTO dbo.Trade ([TradeId], [Amount], ...) OUTPUT inserted.* VALUES (@TradeId, @Amount, ...);
```

Because the statement uses bracket-quoted identifiers, the
`OUTPUT inserted.*` clause and `IF EXISTS` syntax, it is **only valid
on SQL Server**. The operator also adapts itself to ODBC / OLE DB
drivers (positional `?` placeholders).

For provider-agnostic upserts, prefer the
[Entity Framework Core operators](./4_entityFramework.md) — they
handle SQLite / PostgreSQL / MySQL transparently and offer the same
`SeekOn` / `DoNotUpdateIfExists` / `InsertOnly` switches.

## Cheat sheet

| Intent | Snippet |
| --- | --- |
| Run a `SELECT`, stream rows | `CrossApplySqlServerQuery(name, o => o.FromQuery(sql).WithMapping<T>())` |
| Default column → property mapping | `WithMapping<T>()` (no lambda) |
| Custom mapping | `WithMapping<T>(i => new T { X = i.ToColumn<int>("X") })` |
| Run an `INSERT`/`UPDATE`/`DELETE` per row | `ToSqlCommand(name, "INSERT ... @Param ...")` |
| Round-trip generated columns | add `OUTPUT inserted.*` (SQL Server) or `RETURNING ...` (PG/SQLite) |
| Bulk SQL Server upsert | `SqlServerSave(name, o => o.ToTable("X").PivotOn(...))` |
| Provider-agnostic upsert | use `EfCoreSave` from `Paillave.Etl.EntityFrameworkCore` |
