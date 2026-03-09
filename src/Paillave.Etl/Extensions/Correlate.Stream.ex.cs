using System;

namespace Paillave.Etl.Core;

public static partial class CorrelateEx
{
    public static IStream<Correlated<TOut>> CorrelateToSingle<TInLeft, TInRight, TOut>(this IStream<Correlated<TInLeft>> streamLeft, string name, IStream<Correlated<TInRight>> streamRight, Func<TInLeft, TInRight, TOut> resultSelector)
    {
        return new CorrelateToSingleStreamNode<TInLeft, TInRight, TOut>(name, new CorrelateArgs<TInLeft, TInRight, TOut>
        {
            LeftInputStream = streamLeft,
            RightInputStream = streamRight,
            ResultSelector = resultSelector
        }).Output;
    }
    public static IStream<Correlated<TOut>> CorrelateToMany<TInLeft, TInRight, TOut>(this IStream<Correlated<TInLeft>> streamLeft, string name, IStream<Correlated<TInRight>> streamRight, Func<TInLeft, TInRight, TOut> resultSelector)
    {
        return new CorrelateToManyStreamNode<TInLeft, TInRight, TOut>(name, new CorrelateArgs<TInLeft, TInRight, TOut>
        {
            LeftInputStream = streamLeft,
            RightInputStream = streamRight,
            ResultSelector = resultSelector
        }).Output;
    }
}
