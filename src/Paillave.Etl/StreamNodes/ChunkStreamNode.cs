using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class ChunkArgs<TIn>
    {
        public IStream<TIn> InputStream { get; set; }
        public int ChunkSize { get; set; }
    }
    public class ChunkStreamNode<TIn> : StreamNodeBase<IEnumerable<TIn>, IStream<IEnumerable<TIn>>, ChunkArgs<TIn>>
    {
        public ChunkStreamNode(string name, ChunkArgs<TIn> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<IEnumerable<TIn>> CreateOutputStream(ChunkArgs<TIn> args)
        {
            return base.CreateUnsortedStream(args.InputStream.Observable.Chunk(args.ChunkSize));
        }
    }
}
