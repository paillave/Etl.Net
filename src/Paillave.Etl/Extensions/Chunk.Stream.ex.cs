using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.ValuesProviders;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SystemIO = System.IO;

namespace Paillave.Etl.Extensions
{
    public static partial class DistinctEx
    {
        public static IStream<IEnumerable<TIn>> Chunk<TIn>(this IStream<TIn> stream, string name, int chunkSize)
        {
            return new ChunkStreamNode<TIn>(name, new ChunkArgs<TIn>
            {
                ChunkSize = chunkSize,
                InputStream = stream
            }).Output;
        }
    }
}
