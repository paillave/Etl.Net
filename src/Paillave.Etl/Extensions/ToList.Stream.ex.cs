using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core;

public static partial class ToListEx
{
    public static ISingleStream<List<TIn>> ToList<TIn>(this IStream<TIn> stream, string name)
        => new ToListStreamNode<TIn>(name, new ToListArgs<TIn> { InputStream = stream, }).Output;

    public static ISingleStream<Correlated<List<TIn>>> ToList<TIn>(this IStream<Correlated<TIn>> stream, string name)
        => new ToListCorrelatedStreamNode<TIn>(name, new ToListCorrelatedArgs<TIn> { InputStream = stream, }).Output;
}
