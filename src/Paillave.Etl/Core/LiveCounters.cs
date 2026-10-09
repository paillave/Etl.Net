using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Paillave.Etl.Core;

/// <summary>
/// Cheap, lock-free count of the rows that went through each node while a process is running. Unlike
/// <see cref="ExecutionOptions{TConfig}.UseDetailedTraces"/>, no trace event is created per row: the cost is a single
/// <see cref="Interlocked.Increment(ref long)"/> per row and per output stream. Meant to be sampled periodically
/// (<see cref="Snapshot"/>) from another thread to display a progress. Values are approximate while running; the exact
/// final counters remain the ones of <see cref="ExecutionStatus.StreamStatisticCounters"/>.
/// </summary>
public class LiveCounters
{
    internal sealed class Cell
    {
        public long Value;
    }
    private readonly ConcurrentDictionary<string, Cell> _cells = new();
    internal Cell GetCell(string nodeName) => _cells.GetOrAdd(nodeName, _ => new Cell());
    /// <summary>Rows seen so far per node name (the rows of all the output streams of a node are summed up).</summary>
    public IReadOnlyDictionary<string, long> Snapshot()
        => _cells.ToDictionary(i => i.Key, i => Interlocked.Read(ref i.Value.Value));
}
