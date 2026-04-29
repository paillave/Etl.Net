---
sidebar_position: 4
---

# Entity Framework Core

The `Paillave.Etl.EntityFrameworkCore` package plugs an existing
[EF Core](https://learn.microsoft.com/ef/core/) `DbContext` into the
ETL pipeline. It provides operators for **reading** rows from the
database, **writing** rows back, **enriching** the stream by
**looking up** existing entities, and **deleting** rows. Every
operator participates in the same dependency-injection container as
the rest of your job, opens its own `DbContext` for the duration of
each batch, and disposes it cleanly at the end.

> All snippets below are mirrored by tests in
> `src/Paillave.Etl.Tests/DocExamples/EntityFrameworkOperatorsExamplesTests.cs`
> running against an in-memory SQLite database. Each section names
> the matching test for traceability.

## Wiring `DbContext` into the pipeline

ETL.Net resolves a `DbContext` (or a typed sub-class) from the
`IServiceProvider` you pass through `ExecutionOptions.Services`.

```cs
public class DocDbContext(DbContextOptions<DocDbContext> options) : DbContext(options)
{
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Person> People => Set<Person>();
}

var optsBuilder = new DbContextOptionsBuilder<DocDbContext>()
    .UseSqlServer("Server=...;Database=Demo;Integrated Security=true;");

var services = new ServiceCollection()
    .AddTransient<DocDbContext>(_ => new DocDbContext(optsBuilder.Options))
    .AddTransient<DbContext>(sp => sp.GetRequiredService<DocDbContext>())
    .BuildServiceProvider();

await StreamProcessRunner.CreateAndExecuteAsync(
    config,
    job,
    new ExecutionOptions<TConfig> { Services = services });
```

Two important points:

| Concern | Recommendation |
| --- | --- |
| Lifetime | Register the context as **transient**: every operator opens a short-lived scope per batch, and a fresh context per scope avoids `Cannot access a disposed context` errors. |
| Base type | Operators that don't take a `WithContextType<...>()` argument call `services.GetService<DbContext>()`. Always expose the base `DbContext` as well as your concrete type. |

Multi-tenant or sharded scenarios pass a string `keyedConnection`
(`AddKeyedTransient<DbContext>("tenantA", ...)`) and route the
operator to a specific connection with the `WithKeyedConnection`
builder method.

## Reading from the database

### `EfCoreSelect` — load every row matching a query

> Test: `EfCoreSelect_LoadsAllRowsFromTable`

```cs {3}
root
    .EfCoreSelect("read countries",
                  (db, _) => db.Set<Country>())
    .Do("collect", c => Console.WriteLine(c.Code));
```

`EfCoreSelect` runs once per upstream row and pushes every record
returned by the query into the stream. Pass an `IQueryable<T>` —
including filtering, projections, `Include`, `OrderBy`, etc. — and
the operator materialises the result via `ToList()` (or via
`AsQueryable()` if `streamMode: true` is used to keep the underlying
reader open).

### `EfCoreSelectSingle` — one row per input

> Test: `EfCoreSelectSingle_LoadsOnePerInput`

```cs {4-6}
root
    .CrossApply("codes", _ => new[] { "FR", "DE" })
    .EfCoreSelectSingle(
        "lookup country",
        (db, code) => db.Set<Country>().Where(c => c.Code == code))
    .Do("collect", c => Console.WriteLine(c.Name));
```

`EfCoreSelectSingle` calls `FirstOrDefault()` on the query and
forwards exactly one value per input row. It also has an
`ISingleStream<T>` overload that preserves the single-row contract.

For repeated lookups consider `EfCoreLookup` (below) instead — it
caches the matches and avoids one round-trip per input row.

## Writing to the database

### `EfCoreSave` — insert or upsert

> Tests: `EfCoreSave_InsertsNewRows`, `EfCoreSave_SeekOn_UpdatesExistingMatchingRow`

```cs {3}
root
    .CrossApply("rows", _ => streamOfCountries)
    .EfCoreSave("save countries",
                o => o.SeekOn(c => c.Code));
```

The `SeekOn` clause defines the **business key** that identifies an
existing row. The operator buffers rows by `BatchSize` (default
10000), and for every batch:

1. Opens a fresh `DbContext` scope.
2. For each entity, looks up an existing row using the business key.
3. If found, copies its primary key onto the incoming entity and
   issues an `Update`; otherwise issues an `Add`.
4. Calls `SaveChangesAsync` and clears the change tracker.

You can chain several `SeekOn` calls (composite key) or use
`AlternativelySeekOn` (try the alternative key when the primary one
returns nothing). For raw, expression-based matching pass
`SeekOn((db, in) => db.X == in.X && db.Y > in.Y)`.

#### `DoNotUpdateIfExists` — insert if missing, leave existing alone

> Test: `EfCoreSave_DoNotUpdateIfExists_KeepsExistingValuesUnchanged`

```cs {3}
root
    .CrossApply("rows", _ => stream)
    .EfCoreSave("save", o => o
        .SeekOn(c => c.Code)
        .DoNotUpdateIfExists());
```

Ideal for idempotent imports where existing data is curated and the
load step must not overwrite it.

#### `InsertOnly` — bypass the merge logic

```cs
.EfCoreSave("bulk insert", o => o.InsertOnly())
```

Disables the existence check entirely: every row is `Add`-ed, no
matter what the database contains. Useful for append-only tables
(events, ledger entries, audit logs).

#### Bulk vs. EF Core mode

```cs
.EfCoreSave("save", o => o
    .SeekOn(c => c.Code)
    .WithMode(SaveMode.SqlServerBulk))   // default
```

| Mode | Behaviour |
| --- | --- |
| `SaveMode.SqlServerBulk` | Uses `SqlBulkCopy` against SQL Server (huge performance win for large batches); falls back to plain EF Core when the provider is not SQL Server. |
| `SaveMode.EntityFrameworkCore` | Always uses `DbContext.SaveChangesAsync` — provider-agnostic, slower, easier to debug. |

Tune throughput further with `WithBatchSize(int)` and route to a
specific connection with `WithKeyedConnection("name")` /
`WithContextType<MyContext>()`.

#### Custom output projection

By default `EfCoreSave` re-emits the saved entity. Use `Output` to
shape what flows downstream — for instance to keep both the source
row and the persisted ID:

```cs
.EfCoreSave("save trade", o => o
    .Entity(i => new TradeRow { Id = i.SourceId, Amount = i.Amount })
    .SeekOn(t => t.Id)
    .Output((source, saved) => new { Source = source, SavedId = saved.Id }))
```

## Enriching the stream

### `EfCoreLookup` — join the stream with a `DbSet`

> Test: `EfCoreLookup_EnrichesStreamRowsWithEntityFromDb`

```cs {3-6}
root
    .CrossApply("input", _ => users)
    .EfCoreLookup("with country", o => o
        .Set<Country>()
        .On(i => i.CountryCode, c => c.Code)
        .Select((i, c) => $"{i.UserName}:{(c == null ? \"?\" : c.Name)}"));
```

For each input row, `EfCoreLookup` finds the entity in the database
(or in its in-memory cache) whose key matches `i.CountryCode`. The
`Select` lambda receives the input and the matched entity (`null`
when no match was found, exactly like a `LeftJoin`).

By default the operator pre-fetches the **entire** target table into
a dictionary on first use — perfect for reference data with a few
thousand rows. For large tables call `.NoCacheFullDataset()` to
switch to per-row queries with a configurable LRU cache:

```cs
.EfCoreLookup("with city", o => o
    .Set<City>()
    .On(i => i.ZipCode, c => c.Zip)
    .Select((i, c) => i with { CityName = c?.Name })
    .NoCacheFullDataset()
    .CacheSize(10_000));
```

The `Query` overload replaces `Set<T>()` with an arbitrary
`IQueryable<T>` so you can pre-filter or shape the lookup data:

```cs
.Query<Country>(db => db.Set<Country>().Where(c => c.IsActive))
```

#### `CreateIfNotFound` — auto-insert missing reference data

> Test: `EfCoreLookup_CreateIfNotFound_InsertsMissingEntities`

```cs {6-7}
.EfCoreLookup("with country", o => o
    .Set<Country>()
    .On(i => i.CountryCode, c => c.Code)
    .Select((i, c) => $"{i.UserName}:{c!.Name}")
    .NoCacheFullDataset()
    .CreateIfNotFound(i => new Country
    {
        Code = i.CountryCode,
        Name = i.CountryCode + "-auto"
    }));
```

`CreateIfNotFound` runs only when the cached dataset mode is
disabled (`NoCacheFullDataset`). For each input that has no match,
it builds a fresh entity, persists it, and returns it to the
selector — guaranteeing that the second argument of `Select` is
never `null`.

## Deleting rows

### `EfCoreDelete` — remove rows that match a predicate

> Test: `EfCoreDelete_RemovesMatchingRows`

```cs {3-5}
root
    .CrossApply("codes", _ => new[] { "DE", "IT" })
    .EfCoreDelete("delete countries", o => o
        .Set<Country>()
        .Where((code, c) => c.Code == code));
```

For every input row, `EfCoreDelete` translates the
`Where((streamRow, entity) => …)` expression into an
[`ExecuteDelete`](https://learn.microsoft.com/ef/core/saving/execute-insert-update-delete)
SQL statement and runs it server-side — no entities are loaded into
memory. The original stream row is forwarded unchanged so you can
chain further operators (logging, counters, etc.).

## Updating with `EfCoreUpdate`

`EfCoreUpdate` performs a bulk SQL `UPDATE` based on a `SqlBulkCopy`
staging table; it requires a SQL Server provider and is therefore
not exercised by the SQLite-based example tests.

```cs
root
    .CrossApply("rows", _ => updates)
    .EfCoreUpdate("update positions",
        updateKey:    p => new Position { TradeId = p.TradeId },
        updateValues: p => new Position { Amount = p.NewAmount });
```

The two expression trees describe **which** columns identify the
row to update (`updateKey`) and **which** columns to change
(`updateValues`). All other columns are left untouched.

## Cheat sheet

| Intent | Operator |
| --- | --- |
| Read every row matching a query | `EfCoreSelect` |
| Read at most one row per input | `EfCoreSelectSingle` |
| Insert / upsert with business key | `EfCoreSave` + `SeekOn` |
| Insert if missing, never update | `EfCoreSave` + `DoNotUpdateIfExists` |
| Append-only insert | `EfCoreSave` + `InsertOnly` |
| Use `SqlBulkCopy` for huge loads | `EfCoreSave` + `WithMode(SqlServerBulk)` |
| Enrich stream from reference data | `EfCoreLookup` |
| Cache only what's used | `EfCoreLookup` + `NoCacheFullDataset` |
| Auto-create missing reference data | `EfCoreLookup` + `CreateIfNotFound` |
| Server-side delete | `EfCoreDelete` |
| Server-side bulk update (SQL Server only) | `EfCoreUpdate` |
