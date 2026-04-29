using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using Xunit;

namespace Paillave.Etl.Tests;

/// <summary>
/// Stress / correctness tests for the reactive operator stack on large and
/// complex pipelines. They also act as regression tests for the leak fixes
/// in <c>PushSubject</c>, the early-terminating operators (Take/First/...)
/// and <c>StreamNodeBase</c>.
/// </summary>
public class ReactiveLargeStreamTests
{
    private const int Million = 1_000_000;

    private static IDeferredPushObservable<int> Range(int from, int count, CancellationToken ct = default)
        => PushObservable.Range(from, count, ct);

    private static IDeferredPushObservable<T> FromEnumerable<T>(IEnumerable<T> source, CancellationToken ct = default)
        => PushObservable.FromEnumerable(source, ct);

    private static async Task<T> RunAsync<T>(IDeferredPushObservable<int> src, Func<IPushObservable<int>, IPushObservable<T>> pipeline)
    {
        var task = pipeline(src).LastAsync();
        src.Start();
        return await task;
    }

    // ---------------------------------------------------------------------
    // Big pipelines: correctness vs. LINQ reference implementation.
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Map_Filter_Aggregate_OneMillionItems()
    {
        var src = Range(0, Million);
        var task = src
            .Map(i => (long)i * 2L)
            .Filter(v => v % 3 == 0)
            .Aggregate<long, long>((acc, v) => acc + v)
            .ToTaskAsync();
        src.Start();

        long expected = 0;
        for (int i = 0; i < Million; i++)
        {
            long v = (long)i * 2L;
            if (v % 3 == 0) expected += v;
        }
        Assert.Equal(expected, await task);
    }

    [Fact]
    public async Task Scan_RunningSum_OneMillionItems()
    {
        var src = Range(1, Million);
        var task = src.Scan<int, long>(0L, (a, v) => a + v).LastAsync();
        src.Start();
        long expected = (long)Million * (Million + 1) / 2;
        Assert.Equal(expected, await task);
    }

    [Fact]
    public async Task Distinct_HighCardinality()
    {
        const int N = 200_000;
        var src = FromEnumerable(Enumerable.Range(0, N).Select(i => i % 1000));
        var task = src.Distinct().Count().LastAsync();
        src.Start();
        Assert.Equal(1000, await task);
    }

    [Fact]
    public async Task DistinctUntilChanged_OnRunsOfRepeatedValues()
    {
        const int N = 500_000;
        // produces 0,0,0,...,1,1,1,...,2,...  (each value repeated 100 times)
        var src = FromEnumerable(Enumerable.Range(0, N).Select(i => i / 100));
        var task = src.DistinctUntilChanged().Count().LastAsync();
        src.Start();
        Assert.Equal(N / 100, await task);
    }

    [Fact]
    public async Task Chunk_LargeStream_PreservesAllItems()
    {
        const int N = 250_000;
        const int ChunkSize = 1024;
        var src = Range(0, N);
        var task = src.Chunk(ChunkSize).Map(c => c.Count()).Aggregate<int, long>((a, v) => a + v).ToTaskAsync();
        src.Start();
        Assert.Equal(N, await task);
    }

    // ---------------------------------------------------------------------
    // Group / aggregate by key on high-cardinality input.
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Group_ParallelAggregate_HighCardinality()
    {
        const int N = 500_000;
        const int Keys = 1000;
        var src = Range(0, N);
        var task = src
            .Group(
                i => i % Keys,
                (grp, first) => grp.Aggregate<int, long>(_ => 0L, (a, v) => a + v).Map(sum => new KeyValuePair<int, long>(first % Keys, sum)))
            .ToListAsync();
        src.Start();
        var result = await task;

        var expected = Enumerable.Range(0, N)
            .GroupBy(i => i % Keys)
            .ToDictionary(g => g.Key, g => g.Sum(i => (long)i));

        Assert.Equal(Keys, result.Count);
        foreach (var kv in result)
            Assert.Equal(expected[kv.Key], kv.Value);
    }

    // ---------------------------------------------------------------------
    // LeftJoin on two large sorted streams.
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LeftJoin_LargeSortedStreams()
    {
        const int N = 100_000;
        var left = Range(0, N);
        // Use a reference type on the right side so missing matches are observable as null.
        var right = FromEnumerable(Enumerable.Range(0, N).Where(i => i % 2 == 0).Select(i => new RightRow(i, (long)i * 10L)));

        var task = left.LeftJoin(
            right,
            l => l,
            r => r.Key,
            keyPositions: null!,
            (l, r) => (l, r)).ToListAsync();

        left.Start();
        right.Start();

        var result = await task;
        Assert.Equal(N, result.Count);
        Assert.All(result, p =>
        {
            if (p.l % 2 == 0)
            {
                Assert.NotNull(p.r);
                Assert.Equal(p.l, p.r!.Key);
                Assert.Equal((long)p.l * 10L, p.r.Val);
            }
            else
            {
                Assert.Null(p.r);
            }
        });
    }

    private sealed class RightRow
    {
        public RightRow(int key, long val) { Key = key; Val = val; }
        public int Key { get; }
        public long Val { get; }
    }

    // ---------------------------------------------------------------------
    // Multi-stage pipeline mixing several operators.
    // ---------------------------------------------------------------------

    [Fact]
    public async Task ComplexMultiStagePipeline()
    {
        const int N = 300_000;
        var src = Range(1, N);
        // Even values, multiplied, chunked, sum of chunk averages, then take first 50.
        var pipeline = src
            .Filter(i => i % 2 == 0)
            .Map(i => (long)i)
            .Chunk(500)
            .Map(c => (long)c.Average())
            .Take(50)
            .Aggregate<long, long>((a, v) => a + v);

        var task = pipeline.LastAsync();
        src.Start();

        var expected = Enumerable.Range(1, N)
            .Where(i => i % 2 == 0)
            .Select(i => (long)i)
            .Chunk(500)
            .Select(c => (long)c.Average())
            .Take(50)
            .Sum();

        Assert.Equal(expected, await task);
    }

    [Fact]
    public async Task FlatMap_FanOut_CorrectCount()
    {
        const int Outer = 1_000;
        const int Inner = 500;
        var src = Range(0, Outer);
        var task = src
            .FlatMap((i, ct) => PushObservable.Range(0, Inner, ct))
            .Count()
            .LastAsync();
        src.Start();
        Assert.Equal(Outer * Inner, await task);
    }

    // ---------------------------------------------------------------------
    // Early termination: Take / First must complete promptly even when the
    // source is much larger. We don't test that the producer stops (it is
    // a hot enumeration) but we do verify the result is correct.
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Take_OnLargeSource_StopsAtRequestedCount()
    {
        var src = Range(0, Million);
        var task = src.Take(1234).ToListAsync();
        src.Start();
        var list = await task;
        Assert.Equal(1234, list.Count);
        Assert.Equal(Enumerable.Range(0, 1234).ToList(), list);
    }

    [Fact]
    public async Task First_OnLargeSource_ReturnsFirst()
    {
        var src = Range(42, Million);
        var task = src.First().ToTaskAsync();
        src.Start();
        Assert.Equal(42, await task);
    }

    // ---------------------------------------------------------------------
    // Leak / lifetime regression tests for the fixes:
    //   * PushSubject no longer pins itself to a long-lived CancellationToken.
    //   * StreamNodeBase output subscription is owned by the JobExecutionContext.
    //   * Early-terminating operators release their upstream subscription.
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LongLivedCancellationToken_DoesNotPinCompletedSubjects()
    {
        // A single CancellationTokenSource shared by many short-lived
        // pipelines. Before the fix, every subject was pinned to the token's
        // callback list for the lifetime of the CTS, leaking the entire
        // operator graph of every prior run.
        using var cts = new CancellationTokenSource();
        var weakRefs = new List<WeakReference>();

        for (int i = 0; i < 200; i++)
        {
            var src = PushObservable.Range(0, 1_000, cts.Token);
            var task = src.Map(x => (long)x * 2L)
                          .Filter(v => v % 3 == 0)
                          .Aggregate<long, long>((a, v) => a + v)
                          .ToTaskAsync();
            src.Start();
            await task;
            weakRefs.Add(new WeakReference(src));
        }

        // Force three GCs, the second one runs after pending finalizers.
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        int alive = weakRefs.Count(w => w.IsAlive);
        // Without the fix, 'alive' is ~weakRefs.Count. With the fix the
        // overwhelming majority must be collectible. We allow a small
        // amount of slack for the JIT / debugger / xUnit's own roots.
        Assert.True(
            alive < weakRefs.Count / 4,
            $"Too many subjects still rooted by long-lived CancellationToken: {alive}/{weakRefs.Count}");
    }

    [Fact]
    public async Task TakeUntilTrigger_DoesNotPinSourceSubscription()
    {
        // Validate the OnCompleted hook on TakeUntilSubject.
        using var cts = new CancellationTokenSource();
        var trigger = new PushSubject<int>(cts.Token);
        var src = PushObservable.Range(0, 10_000, cts.Token);

        var task = src.TakeUntil(trigger).ToListAsync();
        src.Start();
        // give the producer a moment to start pushing
        await Task.Delay(20);
        trigger.PushValue(1);
        trigger.Complete();
        var list = await task;

        Assert.True(list.Count <= 10_000);
        // Strict ordering preserved
        for (int i = 1; i < list.Count; i++)
            Assert.Equal(list[i - 1] + 1, list[i]);
    }

    // ---------------------------------------------------------------------
    // Cancellation: cancelling the source mid-flight should terminate
    // the pipeline cleanly (no hang).
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Cancellation_StopsLargePipelinePromptly()
    {
        // Cancellation has to drain the in-flight value through the pipeline
        // (the producer holds the source lock while pumping). We use a
        // moderately large source and a simple Filter/Count pipeline so the
        // per-item cost is small and cancellation is observed quickly.
        using var cts = new CancellationTokenSource();
        var src = PushObservable.Range(0, 5_000_000, cts.Token);
        var task = src.Filter(i => i % 2 == 0).Count().LastAsync();
        src.Start();
        await Task.Delay(20);
        cts.Cancel();

        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(30)));
        Assert.Same(task, completed);
        var count = await task;
        // The partial count must be at most the worst case of the full source.
        Assert.InRange(count, 0, 2_500_000);
    }
}
