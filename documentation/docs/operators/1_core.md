---
sidebar_position: 1
---

# Core operators

The Etl.Net **core operators** are the building blocks shipped in
`Paillave.Etl`. They operate on three flavours of stream:

| Stream type                      | Meaning                                                            |
| -------------------------------- | ------------------------------------------------------------------ |
| `IStream<T>`                     | An unsorted, unbounded sequence of `T` rows.                       |
| `ISingleStream<T>`               | A stream guaranteed to push **exactly one** value of type `T`.     |
| `IKeyedStream<T, TKey>`          | An unsorted stream that exposes a key (used as right side of joins).|
| `ISortedStream<T, TKey>`         | A stream sorted on `TKey` (left side of sort‑merge joins).         |
| `IStream<Correlated<T>>`         | A stream whose rows carry a correlation token (see *Correlation*). |

Every operator returns a new stream — the operators are pure
combinators, the only side effects are explicit (`Do`, `ToConnector`,
`ToList`).

:::tip Examples are tested
Every snippet below is mirrored by an `xUnit` test in
[Paillave.Etl.Tests/DocExamples/CoreOperatorsExamplesTests.cs](../../../src/Paillave.Etl.Tests/DocExamples/CoreOperatorsExamplesTests.cs).
The test name is given next to each example so you can run it.
:::

---

## Transformation

### `Select` — project each row

```cs {3}
root.CrossApply("seed", _ => Enumerable.Range(1, 5))
    .Select("double", i => i * 2)
    .Do("collect", Console.WriteLine);
```

| input | output |
| ----- | ------ |
| 1     | 2      |
| 2     | 4      |
| 3     | 6      |
| 4     | 8      |
| 5     | 10     |

`Select` has eight overloads. The most useful variants are:

* `Select(name, Func<TIn, TOut>)` — straight projection.
* `Select(name, Func<TIn, int, TOut>)` — projection with the **row
  index** (0‑based).
* `SelectResolved(name, Func<TIn, IDependencyResolver, TOut>)` —
  projection with access to the dependency injection container of the
  current job.
* `SelectCorrelated(name, …)` — projection that preserves a
  correlation token (see *Correlation*).

> Test: `Select_DoublesEachInteger`, `Select_WithIndex`.

### `Where` — keep rows that match a predicate

```cs {2}
root.CrossApply("seed", _ => Enumerable.Range(1, 10))
    .Where("even only", i => i % 2 == 0)
    .Do("collect", Console.WriteLine);
```

| in    | passes? | out |
| ----- | ------- | --- |
| 1     | no      | —   |
| 2     | yes     | 2   |
| 3     | no      | —   |
| …     | …       | …   |
| 10    | yes     | 10  |

Variants: `Where(name, Func<TIn, int, bool>)` (with index),
`WhereCorrelated(...)`, and a DI‑aware overload.

> Test: `Where_KeepsOnlyMatching`.

### `OfType<TIn, TOut>` — filter by sub‑type

```cs {6}
abstract class Animal { public string Name = ""; }
class Dog : Animal { public string Breed = ""; }
class Cat : Animal { public bool Indoor; }

root.CrossApply("seed", _ => animals)
    .OfType<Animal, Dog>("dogs only")
    .Do("print", d => Console.WriteLine(d.Breed));
```

`OfType` is the strongly‑typed equivalent of `Where(x => x is Dog)
.Select(x => (Dog)x)` collapsed in one node.

> Test: `OfType_FiltersBySubtype`.

### `Fix` — patch missing or wrong values

```cs {4}
root.CrossApply("seed", _ => cities)
    .Fix("default country", f => f
        .FixProperty(c => c.Country).IfNullWith(_ => "??")
        .FixProperty(c => c.Population).AlwaysWith(c => Math.Max(c.Population, 0)))
    .Do("save", repo.Save);
```

`Fix` rewrites individual properties of every row. The fluent builder
exposes:

* `IfNullWith(getNewValue)` — only replace when the current value is
  `null`.
* `IfNotNullWith(...)` — only replace when not `null`.
* `AlwaysWith(...)` — replace unconditionally.

For an `ISingleStream<T>` use `FixNull` (the single‑stream variant —
its semantics are identical, but applies only to nullable singles).

> Test: `Fix_SetsDefaultsForMissingFields`.

### `WithPrevious` — sliding window

```cs {2-7}
root.CrossApply("seed", _ => new[] { 10, 20, 30, 40 })
    .WithPrevious("window 2", 2, window =>
    {
        // window[0] = current row,
        // window[1] = previous row (when present)
        var prev = window.Length >= 2 ? (int?)window[1] : null;
        return (prev, current: window[0]);
    })
    .Do("emit", Console.WriteLine);
```

| step | window     | emitted        |
| ---- | ---------- | -------------- |
| 1    | `[10]`     | `(null, 10)`   |
| 2    | `[20, 10]` | `(10,   20)`   |
| 3    | `[30, 20]` | `(20,   30)`   |
| 4    | `[40, 30]` | `(30,   40)`   |

The buffer is bounded (`count` parameter), so memory stays constant.

> Test: `WithPrevious_ExposesSlidingWindow`.

### `ReKey` — rebuild a `IKeyedStream`/`ISortedStream` view

`ReKey` is needed when you want to feed a stream as the **right** side
of a `Lookup` / `LeftJoin` / `Substract` but the upstream operator only
gave you an `IStream<T>`. It does **not** sort — the caller is
responsible for the underlying ordering when targeting an
`ISortedStream`.

```cs
var keyed = stream.ReKey("rekey", row => row.Id);
```

---

## Generation

### `CrossApply` — one row in, many rows out

```cs {2}
root.CrossApply("seed", _ => new[] { 2, 3 })
    .CrossApply("expand", n => Enumerable.Range(1, n))
    .Do("collect", Console.WriteLine);
```

| input | expansion |
| ----- | --------- |
| 2     | 1, 2      |
| 3     | 1, 2, 3   |

`CrossApply` is the standard way to *seed* a pipeline (you usually
start from `ISingleStream<TConfig>` and emit the first batch of rows
out of a configuration). It also ships with file‑oriented overloads
(`CrossApplyTextFile`, `CrossApplyXmlFile`, `CrossApplyJsonFile`,
`CrossApplyExcelFile`, `CrossApplyZipFile`, `CrossApplyFolderFiles`,
`CrossApplyFtpFiles`, `CrossApplySftpFiles`, …) — these are documented
in the dedicated source‑specific pages.

> Test: `CrossApply_ExpandsRowsOneToMany`.

### `Pivot` — cross‑tabulation

```cs {6-10}
root.CrossApply("seed", _ => rows)
    .Pivot("sum + max",
        getKey: t => t.K,
        aggregation: t => new
        {
            Sum = AggregationOperators.Sum(t.V),
            Max = AggregationOperators.Max(t.V),
        })
    .Do("emit", r => Console.WriteLine($"{r.Key}: {r.Aggregation.Sum}/{r.Aggregation.Max}"));
```

`Pivot` evaluates the lambda **as an expression tree**. Each member of
the anonymous projection must be an `AggregationOperators.*` call:
`Sum`, `Avg`, `Min`, `Max`, `First`, `FirstNotNull`, `Last`. You can
restrict an aggregation to a subset of rows with `.For(condition)`.

| input rows                     | key | Sum | Max |
| ------------------------------ | --- | --- | --- |
| `("A", 1), ("A", 2)`           | A   | 3   | 2   |
| `("B", 5)`                     | B   | 5   | 5   |

> Test: `Pivot_SumAndMaxOnDescriptor`.

---

## Combination

### `UnionAll` — concatenate two streams of the same type

```cs {4}
var left  = root.CrossApply("L", _ => new[] { 1, 2, 3 });
var right = root.CrossApply("R", _ => new[] { 4, 5, 6 });

left.UnionAll("merge", right).Do("emit", Console.WriteLine);
```

Output: `1, 2, 3, 4, 5, 6` (interleaving order is non‑deterministic —
both inputs are pushed concurrently).

`UnionAll` has overloads accepting up to **eight** right inputs and a
*sub‑process* form for fan‑in patterns.

> Test: `UnionAll_ConcatenatesTwoStreams`.

### `Union` — merge sorted streams keeping the order

`Union` is the sorted/keyed counterpart of `UnionAll`. It requires
both inputs to be `ISortedStream<T, TKey>` (or `IKeyedStream`) and
preserves the global ordering. Use it when downstream nodes expect a
sorted stream (e.g. `LeftJoin`, `Aggregate` on sorted, `Sort` skip).

### `Lookup` — enrich the left stream with the right one

```cs {6-10}
people.Lookup("enrich",
    rightStream: countries,
    leftKey:     p => p.CountryCode,
    rightKey:    c => c.Code,
    resultSelector: (p, c) => $"{p.Name} lives in {c.Name}")
    .Do("emit", Console.WriteLine);
```

`Lookup` materialises the **right** stream into an in‑memory hash
table, then streams the **left** side. Memory is `O(|right|)`. There
is no spilling — when the right side is too large for RAM, prefer
`LeftJoin` (sort‑merge).

`Lookup` has three overloads — non‑matching rows can be either
**dropped** (default), **redirected to errors** or **matched against
a default value**.

> Test: `Lookup_EnrichesLeftWithRight`.

### `LeftJoin` — sort‑merge left outer join

```cs {7}
var left  = src1.Sort("sort L", x => x.K);                 // ISortedStream
var right = src2.EnsureKeyed("ensure R keyed", x => x.K);  // IKeyedStream

left.LeftJoin("join", right,
    resultSelector: (l, r) => (l.V, r?.V))
    .Do("emit", Console.WriteLine);
```

Unmatched left rows still flow downstream — `r` will be the
`default(TInRight)` (i.e. `null` for reference types).

`LeftJoin` is the right tool when both sides are very large because it
streams them: memory is bounded by the duplicates of the current key.

> Test: `LeftJoin_KeepsLeftRowsEvenWithoutMatch`.

### `Substract` — set difference

```cs {2}
left.Substract("diff", right, l => l, r => r)
    .Do("emit", Console.WriteLine);
```

Removes from `left` every row whose key matches an entry in `right`.
The unsorted overload (shown above) materialises the right side; the
sorted overload streams both sides like `LeftJoin`.

> Test: `Substract_RemovesRowsPresentInRight`.

### `Combine` — pair `ISingleStream` values together

```cs {3}
var a = pipelineA.EnsureSingle("ensure A");   // ISingleStream<int>
var b = pipelineB.EnsureSingle("ensure B");   // ISingleStream<int>
a.Combine("sum", b, (x, y) => x + y).Do("emit", Console.WriteLine);
```

`CombineAllSingles` exists in arities 1 → 7, so you can fold up to
seven singles into a single tuple/record.

> Test: `Combine_BindsTwoSinglesIntoOne`.

### Correlation — `SetForCorrelation` / `CorrelateToSingle` / `CorrelateToMany`

When a single source row fans out into multiple downstream branches
that you want to **rejoin later** without using a key, tag every row
with `SetForCorrelation`. Each row gets a fresh `Guid` token; the
token is propagated through every operator that sees `Correlated<T>`.

```cs {5,8}
var src = root.CrossApply("seed", _ => rows)
              .SetForCorrelation("tag");

var ids   = src.Select("project id",   r => r.Id);
var names = src.Select("project name", r => r.Name);

ids.CorrelateToSingle("rejoin", names, (id, name) => $"{id}={name}")
   .DoCorrelated("collect", Console.WriteLine);
```

* `CorrelateToSingle` — pairs each left row with the *single* right
  row that shares its correlation token.
* `CorrelateToMany` — pairs with the list of right rows sharing the
  token.
* `Decorrelate` — drops the token (useful before persisting).
* `UnsetForCorrelation` — symmetric to `SetForCorrelation`.

This is what `FinanceTradeImportPipelineTests` uses to enrich four
million trades while staying inside a streaming loop.

> Test: `CorrelateToSingle_PairsTwoStreamsByCorrelationToken`.

---

## Aggregation

### `Aggregate` — fold rows by key

```cs {4-7}
rows.Aggregate(
    "sum per key",
    getKey:            t => t.Item1,
    emptyAggregation:  t => 0,
    aggregate:         (acc, t) => acc + t.Item2)
    .Do("emit", r => Console.WriteLine($"{r.Key}={r.Aggregation}"));
```

The result is a stream of `AggregationResult<TIn, TKey, TAggr>` with
`.Key` and `.Aggregation`. `AggregateMultiKey` exposes the same fold
on a composite key.

> Test: `Aggregate_SumsByKey`.

### `Distinct` — drop duplicates

`Distinct` ships in three flavours:

```cs {1,4,8-10}
rows.Distinct<int, int>("dedup", i => i);                // by key

rows.Distinct("dedup", x => x.Id, true);                  // smart dedup —
                                                          // ignores rows whose
                                                          // key is null

rows.Distinct("merge dups", p => p.Id, b => b             // aggregating dedup
    .ForProperty(p => p.FirstName, DistinctAggregator.FirstNotNull)
    .ForProperty(p => p.LastName,  DistinctAggregator.FirstNotNull));
```

The third form is unique: it merges duplicate rows by aggregating
each property — typical use case is "fill the holes" when several
imports give partial views of the same entity. Available aggregators
are `First`, `FirstNotNull`, `Last`, `Min`, `Max`, `Sum`, `Avg`.

> Tests: `Distinct_RemovesDuplicates`, `Distinct_Smart_FillsMissingFieldsAcrossDuplicates`.

### `GroupBy` — open a sub‑pipeline per group

```cs {4-7}
rows.GroupBy(
    "per key",
    getKey: t => t.Item1,
    subProcess: (subStream, first) => subStream
        .Aggregate("count", _ => first.Item1, _ => 0, (acc, _) => acc + 1)
        .Select("project", r => (r.Key, r.Aggregation)))
    .Do("emit", Console.WriteLine);
```

`GroupBy` has two shapes:

* **`subProcess` shape** (above) — runs an arbitrary mini‑pipeline
  per group. The `first` parameter is the first row of the group, so
  you can carry static info into the sub‑pipeline.
* **single‑aggregator shape** — same surface as `Aggregate` but with
  a result selector taking the whole group at once.

> Test: `GroupBy_WithSubProcess_CountsPerGroup`.

### `ToList` — collapse to a `ISingleStream<List<T>>`

```cs {2}
root.CrossApply("seed", _ => Enumerable.Range(1, 5))
    .ToList("collect")
    .Do("emit", list => Console.WriteLine(list.Count));
```

`ToList` materialises the entire upstream into RAM. Use it when the
downstream needs the full collection (typical in tests, or when you
need a `List<T>` to feed `Lookup` against an external store).

> Test: `ToList_CollectsTheStreamIntoASingle`.

### `Chunk` — batch rows by size

```cs {2}
rows.Chunk("by 100", 100)            // IStream<IEnumerable<T>>
    .Do("save", batch => repo.SaveBatch(batch));
```

Useful before bulk database inserts.

> Test: `Chunk_BatchesRows`.

---

## Ordering / Selection

### `Sort` — block the stream until ordered

```cs
rows.Sort("by id", r => r.Id);                // ascending
rows.Sort("by id desc", r => r.Id, SortOrder.Desc);
```

`Sort` is **blocking**: it has to see every row before emitting the
first. Memory is `O(|rows|)`. Prefer `EnsureSorted` when the source
is *known* to be sorted already (e.g. coming from an `ORDER BY` SQL
query).

### `EnsureSorted` / `EnsureKeyed`

Promotes the static type of an `IStream<T>` to `ISortedStream<T, TK>`
(or `IKeyedStream`) **without** sorting at runtime. They throw at
runtime if the assertion is violated.

### `Top` / `Skip` / `First` / `Last`

```cs
rows.Top("first 3", 3);             // keeps the first 3 rows
rows.Skip("skip 2", 2);             // drops the first 2 rows
rows.First("first").Do(...);        // → ISingleStream<T>
rows.Last("last") .Do(...);         // → ISingleStream<T>
```

> Tests: `Top_KeepsFirstN`, `Skip_DropsFirstN`,
> `First_PromotesFirstRowToSingleStream`.

### `EnsureSingle` — turn an `IStream<T>` into an `ISingleStream<T>`

Throws at runtime if more than one row reaches it.

---

## Side effects

### `Do` — observe each row

```cs {2}
rows
    .Do("log", r => logger.LogInformation("row {@row}", r))
    .Where("…", …);
```

`Do` does not change the stream — its output is the same `IStream<T>`
as the input. Eight overloads cover the index/Correlated/DI‑aware
variants. Use it for logging, metrics and feeding observers.

> Test: `Do_RunsForEveryRow`.

---

## Process control

### `SubProcess` — encapsulate a piece of pipeline

`SubProcess` is the recommended way to share a non‑trivial piece of
pipeline between several jobs. The wrapped lambda receives the
upstream as an `ISingleStream<TUpstream>` and must return any stream
shape (often `IStream<TOut>`). Use it to scope tracing nodes and to
emit a single statistics row at the end of a batch.

### `WaitWhenDone` — synchronisation barrier

`WaitWhenDone` blocks until both the receiver and a side stream have
completed. Useful when a downstream side effect must wait for the
production of a metadata file (e.g. `WaitWhenDone(metadataReady)`
before sending the success email).

### `GetStreamStatistics` / `KeepLastTracesPerNode`

Two diagnostic helpers used by `ExecutionToolkit` and
`Paillave.Etl.Tests`.

* `GetStreamStatistics` adds a counter‑emitting branch that produces
  one row per tracked node when the pipeline finishes.
* `KeepLastTracesPerNode` keeps the last `N` rows seen by every node;
  invaluable when investigating production failures.

---

## Connectors

`Connectors` exposes two helpers to plug a pipeline into the
`Paillave.Etl.FromConfigurationConnectors` runtime:

```cs
var files = root.FromConnector<IFileValue>("input", config.Source);
files.ToConnector("output", config.Sink, fv => fv.Name);
```

This is how the JSON/YAML configurable jobs in
`Paillave.Etl.FromConfigurationConnectors` are wired.

---

## Cheat‑sheet

| You want…                                        | Operator                                |
| ------------------------------------------------ | --------------------------------------- |
| Project rows                                     | `Select`, `SelectResolved`              |
| Filter rows                                      | `Where`, `OfType`                       |
| Patch missing values                             | `Fix`                                   |
| Window over the last *N* rows                    | `WithPrevious`                          |
| Generate rows from a config                      | `CrossApply`                            |
| Join (small right side)                          | `Lookup`                                |
| Join (big right side, ordered)                   | `LeftJoin` + `Sort`/`EnsureSorted`      |
| Split + rejoin without a natural key             | `SetForCorrelation` + `CorrelateToSingle` |
| Group + fold                                     | `Aggregate` / `GroupBy` / `Pivot`       |
| Drop duplicates                                  | `Distinct`                              |
| Persist all rows in RAM                          | `ToList`                                |
| Bulk‑insert by N                                 | `Chunk`                                 |
| Order rows                                       | `Sort`, `EnsureSorted`                  |
| Trim head/tail                                   | `Top`, `Skip`, `First`, `Last`          |
| Side effect (log, save, …)                       | `Do`                                    |
| Encapsulate a reusable mini‑pipeline             | `SubProcess`                            |
| Wait for a parallel branch to finish             | `WaitWhenDone`                          |

For deeper recipes (file‑based ETL, EF Core integration, sorting
strategies, error handling), browse the
[Recipes](../recipes/_category_.json) section.
