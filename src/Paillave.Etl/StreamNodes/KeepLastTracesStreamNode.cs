using System.Linq;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.StreamNodes
{
    public class KeepLastTracesArgs
    {
        public IStream<TraceEvent> InputStream { get; set; }
        public int Limit { get; set; } = 1000;
    }
    public class KeepLastTracesStreamNode : StreamNodeBase<NodeTraces, IStream<NodeTraces>, KeepLastTracesArgs>
    {
        private class Counter
        {
            private int _value = 0;
            public int Value => _value;
            public void Increment() => Interlocked.Increment(ref _value);
        }
        public KeepLastTracesStreamNode(string name, KeepLastTracesArgs args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override IStream<NodeTraces> CreateOutputStream(KeepLastTracesArgs args)
            => base.CreateUnsortedStream(args.InputStream.Observable
                .Group(
                    traceEvent => traceEvent.NodeName,
                    traceEventStream => traceEventStream.Aggregate(
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
}