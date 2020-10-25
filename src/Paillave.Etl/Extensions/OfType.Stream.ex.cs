using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;
using System;

namespace Paillave.Etl.Extensions
{
    public static partial class OfTypeEx
    {
        public static IStream<TOut> OfType<TIn, TOut>(this IStream<TIn> stream, string name) where TOut:TIn
        {
            return new OfTypeStreamNode<TIn, TOut>(name, new OfTypeArgs<TIn, TOut>
            {
                Stream = stream,
            }).Output;
        }
    }
}
