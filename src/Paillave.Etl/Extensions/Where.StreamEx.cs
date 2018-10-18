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
    public static partial class WhereEx
    {
        public static IKeyedStream<TIn, TKey> Where<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new WhereArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Input = stream,
                Predicate = predicate
            }).Output;
        }
        public static ISortedStream<TIn, TKey> Where<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new WhereArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Input = stream,
                Predicate = predicate
            }).Output;
        }
        public static IStream<TIn> Where<TIn>(this IStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereStreamNode<TIn, IStream<TIn>>(name, new WhereArgs<TIn, IStream<TIn>>
            {
                Input = stream,
                Predicate = predicate
            }).Output;
        }
    }
}
