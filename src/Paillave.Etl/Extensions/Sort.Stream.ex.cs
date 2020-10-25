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
    public static partial class SortEx
    {
        public static ISortedStream<TIn, TKey> Sort<TIn, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, object keyPositions = null)
        {
            return new SortStreamNode<TIn, TKey>(name, new SortArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = SortDefinition.Create(getKey, keyPositions)
            }).Output;
        }
        public static ISortedStream<TIn, TKey> Sort<TIn, TKey>(this IStream<TIn> stream, string name, SortDefinition<TIn, TKey> sortDefinition)
        {
            return new SortStreamNode<TIn, TKey>(name, new SortArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = sortDefinition
            }).Output;
        }
        public static ISortedStream<Correlated<TIn>, TKey> Sort<TIn, TKey>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKey> getKey, object keyPositions = null)
        {
            return new SortStreamNode<Correlated<TIn>, TKey>(name, new SortArgs<Correlated<TIn>, TKey>
            {
                Input = stream,
                SortDefinition = SortDefinition.Create((Correlated<TIn> i) => getKey(i.Row), keyPositions)
            }).Output;
        }
    }
}
