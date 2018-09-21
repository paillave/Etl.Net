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
    public static partial class StreamEx
    {
        public static ISortedStream<TIn, TKey> Skip<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, int count)
        {
            return new SkipStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new SkipArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> Skip<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, int count)
        {
            return new SkipStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new SkipArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
        public static IStream<TIn> Skip<TIn>(this IStream<TIn> stream, string name, int count)
        {
            return new SkipStreamNode<TIn, IStream<TIn>>(name, new SkipArgs<TIn, IStream<TIn>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
    }
}
