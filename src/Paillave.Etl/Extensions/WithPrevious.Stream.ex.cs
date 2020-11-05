using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;
using System;

namespace Paillave.Etl.Extensions
{
    public static partial class WithPreviousEx
    {
        public static IStream<TOut> WithPrevious<TIn, TOut>(this IStream<TIn> stream, string name, int count, Func<TIn[], TOut> getResult)
        {
            return new WithPreviousStreamNode<TIn, TOut>(name, new WithPreviousArgs<TIn, TOut>
            {
                Stream = stream,
                GetResult = getResult,
                Count = count
            }).Output;
        }
        public static IStream<Correlated<TOut>> WithPrevious<TIn, TOut>(this IStream<Correlated<TIn>> stream, string name, int count, Func<TIn[], TOut> getResult)
        {
            return new WithPreviousCorrelatedStreamNode<TIn, TOut>(name, new WithPreviousCorrelatedArgs<TIn, TOut>
            {
                Stream = stream,
                GetResult = getResult,
                Count = count
            }).Output;
        }
    }
}
