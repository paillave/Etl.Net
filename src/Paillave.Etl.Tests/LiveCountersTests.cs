using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Xunit;

namespace Paillave.Etl.Tests;

public class LiveCountersTests
{
    [Fact]
    public async Task LiveCounters_MatchFinalCounters()
    {
        var liveCounters = new LiveCounters();
        var runner = StreamProcessRunner.Create<int>(root => root
            .CrossApply("gen", _ => Enumerable.Range(0, 1000))
            .Where("evens", i => i % 2 == 0));

        var status = await runner.ExecuteAsync(0, new ExecutionOptions<int> { LiveCounters = liveCounters });

        Assert.False(status.Failed);
        var live = liveCounters.Snapshot();
        Assert.Equal(1000, live["gen"]);
        Assert.Equal(500, live["evens"]);
        // The exact final counters (trace events) and the live ones agree once the process is over.
        foreach (var finalCounter in status.StreamStatisticCounters.GroupBy(i => i.SourceNodeName))
            Assert.Equal(finalCounter.Sum(i => i.Counter), live[finalCounter.Key]);
    }

    [Fact]
    public async Task LiveCounters_AreVisibleWhileTheProcessIsStillRunning()
    {
        var liveCounters = new LiveCounters();
        using var release = new ManualResetEventSlim(false);
        var runner = StreamProcessRunner.Create<int>(root => root
            .CrossApply("gen", _ => SlowSource(release)));

        var running = runner.ExecuteAsync(0, new ExecutionOptions<int> { LiveCounters = liveCounters });
        try
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < deadline && !(liveCounters.Snapshot().TryGetValue("gen", out var seen) && seen >= 100))
                await Task.Delay(20);
            Assert.False(running.IsCompleted);
            Assert.True(liveCounters.Snapshot()["gen"] >= 100);
        }
        finally { release.Set(); }
        var status = await running;

        Assert.False(status.Failed);
        Assert.Equal(150, liveCounters.Snapshot()["gen"]);
    }

    private static IEnumerable<int> SlowSource(ManualResetEventSlim release)
    {
        for (var i = 0; i < 100; i++) yield return i;
        release.Wait(TimeSpan.FromSeconds(15));
        for (var i = 100; i < 150; i++) yield return i;
    }

    [Fact]
    public async Task WithoutLiveCounters_NothingChanges()
    {
        var runner = StreamProcessRunner.Create<int>(root => root.CrossApply("gen", _ => Enumerable.Range(0, 10)));
        var status = await runner.ExecuteAsync(0);
        Assert.False(status.Failed);
        Assert.Equal(10, status.StreamStatisticCounters.Single(i => i.SourceNodeName == "gen").Counter);
    }
}
