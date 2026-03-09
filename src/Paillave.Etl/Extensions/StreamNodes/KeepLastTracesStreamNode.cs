using System.Linq;
using System.Threading;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core;

public class KeepLastTracesArgs
{
    public IStream<TraceEvent> InputStream { get; set; }
    public int Limit { get; set; } = 1000;
}
public class KeepLastTracesStreamNode(string name, KeepLastTracesArgs args) : StreamNodeBase<NodeTraces, IStream<NodeTraces>, KeepLastTracesArgs>(name, args)
{
    private class Counter
    {
        private int _value = 0;
        public int Value => _value;
        public void Increment() => Interlocked.Increment(ref _value);
    }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override IStream<NodeTraces> CreateOutputStream(KeepLastTracesArgs args)
        => base.CreateUnsortedStream(args.InputStream.Observable
            .Group(
                traceEvent => traceEvent.NodeName,
                (traceEventStream, _) => traceEventStream.Aggregate(
                    i => new
                    {
                        NodeName = i.NodeName,
                        TraceQueue = new LimitedQueue<TraceEvent>(args.Limit),
                        Counter = new Counter()
                    },
                    (o, i) =>
                    {
                        o.TraceQueue.Enqueue(i);
                        o.Counter.Increment();
                        return o;
                    }))
            .Map(i => new NodeTraces
            {
                NodeName = i.NodeName,
                TraceEvents = i.TraceQueue.ToList(),
                ActualCount = i.Counter.Value
            }));
}