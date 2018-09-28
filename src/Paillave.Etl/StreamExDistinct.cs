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

namespace Paillave.Etl
{
    public static partial class StreamExDistinct
    {
        public static IStream<TIn> Distinct<TIn, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey)
        {
            return new DistinctStreamNode<TIn, TKey>(name, new DistinctArgs<TIn, TKey>
            {
                GetKey = getKey,
                InputStream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> Distinct<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name)
        {
            return new DistinctSortedStreamNode<TIn, TKey>(name, new DistinctSortedArgs<TIn, TKey>
            {
                InputStream = stream
            }).Output;
        }
    }
}
