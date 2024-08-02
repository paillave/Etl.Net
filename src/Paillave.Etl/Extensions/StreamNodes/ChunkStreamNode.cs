using Paillave.Etl.Reactive.Operators;
using System.Collections.Generic;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class ChunkArgs<TIn>
    {
        public IStream<TIn> InputStream { get; set; }
        public int ChunkSize { get; set; }
    }
    public class ChunkStreamNode<TIn>(string name, ChunkArgs<TIn> args) : StreamNodeBase<IEnumerable<TIn>, IStream<IEnumerable<TIn>>, ChunkArgs<TIn>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<IEnumerable<TIn>> CreateOutputStream(ChunkArgs<TIn> args)
        {
            return base.CreateUnsortedStream(args.InputStream.Observable.Chunk(args.ChunkSize));
        }
    }
    public class ChunkCorrelatedArgs<TIn>
    {
        public IStream<Correlated<TIn>> InputStream { get; set; }
        public int ChunkSize { get; set; }
    }
    public class ChunkCorrelatedStreamNode<TIn> : StreamNodeBase<Correlated<IEnumerable<TIn>>, IStream<Correlated<IEnumerable<TIn>>>, ChunkCorrelatedArgs<TIn>>
    {
        public ChunkCorrelatedStreamNode(string name, ChunkCorrelatedArgs<TIn> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<Correlated<IEnumerable<TIn>>> CreateOutputStream(ChunkCorrelatedArgs<TIn> args)
        {
            return base.CreateUnsortedStream(args.InputStream.Observable.Chunk(args.ChunkSize).Map(i => new Correlated<IEnumerable<TIn>>
            {
                CorrelationKeys = i.SelectMany(j => j.CorrelationKeys).ToHashSet(),
                Row = i.Select(j => j.Row).ToList()
            }));
        }
    }
}
