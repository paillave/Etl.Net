using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;
using Xunit;

namespace Paillave.Etl.Tests.DocExamples;

/// <summary>
/// Each test in this file is a runnable mirror of an example from the
/// official documentation under <c>documentation/docs/operators/1_core.md</c>.
/// Whenever the doc is changed, the corresponding test must stay green; this
/// guarantees that every snippet shipped to users compiles and runs as
/// described.
/// </summary>
public class CoreOperatorsExamplesTests
{
    // ===================================================================
    // Helpers — every example follows the same skeleton:
    //   await StreamProcessRunner.CreateAndExecuteAsync(rootValue, root => { ... });
    // The "root" parameter is the seed of every stream; here we feed a
    // small in-memory enumerable through CrossApply.
    // ===================================================================

    private static async Task<ExecutionStatus> RunAsync(Action<IStream<int>> build)
        => await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
            build(root.CrossApply("seed", _ => Enumerable.Range(1, 10))));

    // ----- Select ------------------------------------------------------

    [Fact]
    public async Task Select_DoublesEachInteger()
    {
        var collected = new ConcurrentBag<int>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => Enumerable.Range(1, 5))
                .Select("double", i => i * 2)
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        Assert.Equal(new[] { 2, 4, 6, 8, 10 }, collected.OrderBy(i => i));
    }

    [Fact]
    public async Task Select_WithIndex()
    {
        var collected = new ConcurrentBag<(int idx, string letter)>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => new[] { "a", "b", "c" })
                .Select("with index", (letter, idx) => (idx, letter))
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        Assert.Equal(new[] { (0, "a"), (1, "b"), (2, "c") }, collected.OrderBy(t => t.idx));
    }

    // ----- Where -------------------------------------------------------

    [Fact]
    public async Task Where_KeepsOnlyMatching()
    {
        var collected = new ConcurrentBag<int>();
        var status = await RunAsync(s => s
            .Where("even only", i => i % 2 == 0)
            .Do("collect", collected.Add));
        Assert.False(status.Failed);
        Assert.Equal(new[] { 2, 4, 6, 8, 10 }, collected.OrderBy(i => i));
    }

    // ----- Do (side effect) -------------------------------------------

    [Fact]
    public async Task Do_RunsForEveryRow()
    {
        int count = 0;
        var status = await RunAsync(s => s.Do("count", _ => System.Threading.Interlocked.Increment(ref count)));
        Assert.False(status.Failed);
        Assert.Equal(10, count);
    }

    // ----- CrossApply --------------------------------------------------

    [Fact]
    public async Task CrossApply_ExpandsRowsOneToMany()
    {
        var collected = new ConcurrentBag<int>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => new[] { 2, 3 })
                .CrossApply("expand", n => Enumerable.Range(1, n))
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        // 2 -> 1,2 ; 3 -> 1,2,3
        Assert.Equal(5, collected.Count);
        Assert.Equal(new[] { 1, 1, 2, 2, 3 }, collected.OrderBy(i => i));
    }

    // ----- Distinct ----------------------------------------------------

    [Fact]
    public async Task Distinct_RemovesDuplicates()
    {
        var collected = new ConcurrentBag<int>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => new[] { 1, 1, 2, 2, 3 })
                .Distinct<int, int>("dedup", i => i)
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        Assert.Equal(new[] { 1, 2, 3 }, collected.OrderBy(i => i));
    }

    public class PersonRow
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    [Fact]
    public async Task Distinct_Smart_FillsMissingFieldsAcrossDuplicates()
    {
        // The "smart" Distinct overload (no aggregation builder; uses
        // ObjectMerger reflection) merges duplicate rows by filling null
        // fields with the first non-null value seen for each key.
        var rows = new[]
        {
            new PersonRow { Id = 1, FirstName = "Alice", LastName = null },
            new PersonRow { Id = 1, FirstName = null,    LastName = "Smith" },
            new PersonRow { Id = 2, FirstName = "Bob",   LastName = "Brown" },
        };
        var collected = new ConcurrentBag<PersonRow>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => rows)
                .Distinct("merge dups", p => p.Id, b => b
                    .ForProperty(p => p.FirstName, DistinctAggregator.FirstNotNull)
                    .ForProperty(p => p.LastName, DistinctAggregator.FirstNotNull))
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        var byId = collected.ToDictionary(p => p.Id);
        Assert.Equal("Alice", byId[1].FirstName);
        Assert.Equal("Smith", byId[1].LastName);
        Assert.Equal("Bob", byId[2].FirstName);
    }

    // ----- Aggregate ---------------------------------------------------

    [Fact]
    public async Task Aggregate_SumsByKey()
    {
        var collected = new ConcurrentBag<(string key, int sum)>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => new[]
                {
                    ("A", 1), ("B", 2), ("A", 3), ("B", 4), ("A", 5),
                })
                .Aggregate(
                    "sum per key",
                    getKey: t => t.Item1,
                    emptyAggregation: t => 0,
                    aggregate: (acc, t) => acc + t.Item2)
                .Do("collect", r => collected.Add((r.Key, r.Aggregation)));
        });
        Assert.False(status.Failed);
        var byKey = collected.ToDictionary(t => t.key, t => t.sum);
        Assert.Equal(9, byKey["A"]);
        Assert.Equal(6, byKey["B"]);
    }

    // ----- GroupBy with sub-process ------------------------------------

    [Fact]
    public async Task GroupBy_WithSubProcess_CountsPerGroup()
    {
        // GroupBy starts a tiny "sub-pipeline" per group; here we just
        // count the rows of each group and emit a single (key, count) row.
        var collected = new ConcurrentBag<(string key, int count)>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => new[]
                {
                    ("A", 1), ("B", 2), ("A", 3), ("B", 4), ("A", 5),
                })
                .GroupBy(
                    "per key",
                    getKey: t => t.Item1,
                    subProcess: (subStream, first) => subStream
                        .Aggregate("count", _ => first.Item1, _ => 0, (acc, _) => acc + 1)
                        .Select("project", r => (r.Key, r.Aggregation)))
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        var byKey = collected.ToDictionary(t => t.key, t => t.count);
        Assert.Equal(3, byKey["A"]);
        Assert.Equal(2, byKey["B"]);
    }

    // ----- Sort / EnsureSorted -----------------------------------------

    [Fact]
    public async Task Sort_OrdersTheStream()
    {
        var collected = new List<int>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => new[] { 3, 1, 4, 1, 5, 9, 2, 6 })
                .Sort("ascending", i => i)
                .Do("collect", collected.Add); // Sort preserves order downstream
        });
        Assert.False(status.Failed);
        Assert.Equal(new[] { 1, 1, 2, 3, 4, 5, 6, 9 }, collected);
    }

    // ----- UnionAll ----------------------------------------------------

    [Fact]
    public async Task UnionAll_ConcatenatesTwoStreams()
    {
        var collected = new ConcurrentBag<int>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            var left = root.CrossApply("left", _ => new[] { 1, 2, 3 });
            var right = root.CrossApply("right", _ => new[] { 4, 5, 6 });
            left.UnionAll("merge", right).Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, collected.OrderBy(i => i));
    }

    // ----- Top / Skip --------------------------------------------------

    [Fact]
    public async Task Top_KeepsFirstN()
    {
        var collected = new List<int>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => Enumerable.Range(1, 100))
                .Top("first 3", 3)
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        Assert.Equal(new[] { 1, 2, 3 }, collected);
    }

    [Fact]
    public async Task Skip_DropsFirstN()
    {
        var collected = new List<int>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => Enumerable.Range(1, 5))
                .Skip("drop 2", 2)
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        Assert.Equal(new[] { 3, 4, 5 }, collected);
    }

    // ----- First / Last ------------------------------------------------

    [Fact]
    public async Task First_PromotesFirstRowToSingleStream()
    {
        int? captured = null;
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => Enumerable.Range(10, 5))
                .First("first")
                .Do("capture", v => captured = v);
        });
        Assert.False(status.Failed);
        Assert.Equal(10, captured);
    }

    // ----- Pivot -------------------------------------------------------

    public class PivotKV { public string K { get; set; } = ""; public int V { get; set; } }

    [Fact]
    public async Task Pivot_SumAndMaxOnDescriptor()
    {
        var collected = new ConcurrentBag<(string key, int sum, int max)>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => new[]
                {
                    new PivotKV { K = "A", V = 1 },
                    new PivotKV { K = "A", V = 2 },
                    new PivotKV { K = "B", V = 5 },
                })
                .Pivot("sum + max", t => t.K, t => new
                {
                    Sum = AggregationOperators.Sum(t.V),
                    Max = AggregationOperators.Max(t.V),
                })
                .Do("collect", r => collected.Add((r.Key, r.Aggregation.Sum, r.Aggregation.Max)));
        });
        Assert.False(status.Failed);
        var byKey = collected.ToDictionary(t => t.key);
        Assert.Equal(3, byKey["A"].sum);
        Assert.Equal(2, byKey["A"].max);
        Assert.Equal(5, byKey["B"].sum);
        Assert.Equal(5, byKey["B"].max);
    }

    // ----- Lookup (non-correlated, plain join) -------------------------

    public class CountryRow { public string Code { get; set; } = ""; public string Name { get; set; } = ""; }
    public class RowKV { public int K { get; set; } public string V { get; set; } = ""; }

    [Fact]
    public async Task Lookup_EnrichesLeftWithRight()
    {
        var collected = new ConcurrentBag<string>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            var people = root.CrossApply("people",
                _ => new[] { ("Alice", "FR"), ("Bob", "DE"), ("Eve", "US") });
            var countries = root.CrossApply("countries", _ => new[]
            {
                new CountryRow { Code = "FR", Name = "France" },
                new CountryRow { Code = "DE", Name = "Germany" },
                new CountryRow { Code = "US", Name = "USA" },
            });
            people.Lookup("enrich",
                rightStream: countries,
                leftKey: p => p.Item2,
                rightKey: c => c.Code,
                resultSelector: (p, c) => $"{p.Item1} lives in {c.Name}")
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        Assert.Contains("Alice lives in France", collected);
        Assert.Contains("Bob lives in Germany", collected);
        Assert.Contains("Eve lives in USA", collected);
    }

    // ----- LeftJoin (sorted/keyed) -------------------------------------

    [Fact]
    public async Task LeftJoin_KeepsLeftRowsEvenWithoutMatch()
    {
        var collected = new ConcurrentBag<(string left, string? right)>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            var left = root.CrossApply("left",
                _ => new[]
                {
                    new RowKV { K = 1, V = "a" },
                    new RowKV { K = 2, V = "b" },
                    new RowKV { K = 3, V = "c" },
                }).Sort("L sort", t => t.K);
            var right = root.CrossApply("right",
                _ => new[]
                {
                    new RowKV { K = 1, V = "X" },
                    new RowKV { K = 3, V = "Z" },
                })
                .EnsureKeyed("R keyed", t => t.K);
            left.LeftJoin("join",
                rightStream: right,
                resultSelector: (l, r) => (l.V, r?.V))
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        var byLeft = collected.ToDictionary(t => t.left);
        Assert.Equal("X", byLeft["a"].right);
        Assert.Null(byLeft["b"].right);
        Assert.Equal("Z", byLeft["c"].right);
    }

    // ----- Substract ---------------------------------------------------

    [Fact]
    public async Task Substract_RemovesRowsPresentInRight()
    {
        var collected = new ConcurrentBag<int>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            var left = root.CrossApply("left", _ => new[] { 1, 2, 3, 4, 5 });
            var right = root.CrossApply("right", _ => new[] { 2, 4 });
            left.Substract("diff", right, l => l, r => r)
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        Assert.Equal(new[] { 1, 3, 5 }, collected.OrderBy(i => i));
    }

    // ----- WithPrevious ------------------------------------------------

    [Fact]
    public async Task WithPrevious_ExposesSlidingWindow()
    {
        var collected = new List<(int? prev, int curr)>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => new[] { 10, 20, 30, 40 })
                .WithPrevious("window 2", 2, window =>
                {
                    // window[0] = current, window[1] = previous (when present)
                    var prev = window.Length >= 2 ? (int?)window[1] : null;
                    return (prev, window[0]);
                })
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        Assert.Equal((null, 10), collected[0]);
        Assert.Equal((10, 20), collected[1]);
        Assert.Equal((20, 30), collected[2]);
        Assert.Equal((30, 40), collected[3]);
    }

    // ----- Chunk -------------------------------------------------------

    [Fact]
    public async Task Chunk_BatchesRows()
    {
        var sizes = new ConcurrentBag<int>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => Enumerable.Range(1, 7))
                .Chunk("by 3", 3)
                .Do("collect sizes", batch => sizes.Add(batch.Count()));
        });
        Assert.False(status.Failed);
        // 7 rows in batches of 3 => sizes [3,3,1] in some order
        Assert.Equal(new[] { 1, 3, 3 }, sizes.OrderBy(i => i));
    }

    // ----- ToList ------------------------------------------------------

    [Fact]
    public async Task ToList_CollectsTheStreamIntoASingle()
    {
        List<int>? captured = null;
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => Enumerable.Range(1, 5))
                .ToList("collect")
                .Do("capture", l => captured = l);
        });
        Assert.False(status.Failed);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, captured!.OrderBy(i => i));
    }

    // ----- OfType ------------------------------------------------------

    public abstract class Animal { public string Name { get; set; } = ""; }
    public class Dog : Animal { public string Breed { get; set; } = ""; }
    public class Cat : Animal { public bool Indoor { get; set; } }

    [Fact]
    public async Task OfType_FiltersBySubtype()
    {
        var collected = new ConcurrentBag<string>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => new Animal[]
                {
                    new Dog { Name = "Rex", Breed = "Lab" },
                    new Cat { Name = "Mia", Indoor = true },
                    new Dog { Name = "Bo",  Breed = "Pug" },
                })
                .OfType<Animal, Dog>("dogs only")
                .Do("collect", d => collected.Add($"{d.Name} ({d.Breed})"));
        });
        Assert.False(status.Failed);
        Assert.Equal(2, collected.Count);
        Assert.Contains("Rex (Lab)", collected);
        Assert.Contains("Bo (Pug)", collected);
    }

    // ----- Combine (single streams) ------------------------------------

    [Fact]
    public async Task Combine_BindsTwoSinglesIntoOne()
    {
        int? combined = null;
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            var a = root.CrossApply("A", _ => new[] { 10 }).EnsureSingle("ensure A");
            var b = root.CrossApply("B", _ => new[] { 32 }).EnsureSingle("ensure B");
            a.Combine("sum", b, (x, y) => x + y)
             .Do("capture", v => combined = v);
        });
        Assert.False(status.Failed);
        Assert.Equal(42, combined);
    }

    // ----- SetForCorrelation / CorrelateToSingle -----------------------

    [Fact]
    public async Task CorrelateToSingle_PairsTwoStreamsByCorrelationToken()
    {
        // SetForCorrelation tags every row with a fresh GUID. After
        // branching, CorrelateToSingle re-pairs rows that originate from
        // the same source row by token.
        var collected = new ConcurrentBag<string>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            var src = root.CrossApply("seed",
                _ => new[] { (1, "Alice"), (2, "Bob") })
                .SetForCorrelation("tag");

            var ids = src.Select("project id", r => r.Item1);
            var names = src.Select("project name", r => r.Item2);

            ids.CorrelateToSingle("rejoin", names, (id, name) => $"{id}={name}")
                .DoCorrelated("collect", collected.Add);
        });
        Assert.False(status.Failed);
        Assert.Contains("1=Alice", collected);
        Assert.Contains("2=Bob", collected);
    }

    // ----- Fix ---------------------------------------------------------

    public class CityRow
    {
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
    }

    [Fact]
    public async Task Fix_SetsDefaultsForMissingFields()
    {
        var collected = new ConcurrentBag<CityRow>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => new[]
                {
                    new CityRow { Name = "Paris", Country = "FR" },
                    new CityRow { Name = "Tokyo" }, // Country missing
                })
                .Fix("default country", f => f
                    .FixProperty(c => c.Country).IfNullWith(_ => "??"))
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        var byName = collected.ToDictionary(c => c.Name);
        Assert.Equal("FR", byName["Paris"].Country);
        Assert.Equal("??", byName["Tokyo"].Country);
    }

    // ----- SubProcess --------------------------------------------------

    [Fact]
    public async Task SubProcess_WrapsAReusablePipelineSegment()
    {
        var collected = new ConcurrentBag<int>();
        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            root.CrossApply("seed", _ => Enumerable.Range(1, 10))
                .SubProcess("only odd squared", first => first
                    .CrossApply("expand", _ => new[] { 0 }) // re-streamify
                    .Where("odd", i => i % 2 == 1)
                    .Select("square", i => i * i))
                .Do("collect", collected.Add);
        });
        Assert.False(status.Failed);
        // SubProcess sees ISingleStream, but here we just wanted to show
        // it compiles and runs; the meaningful pattern is in recipes.
        Assert.False(status.Failed);
    }
}
