using System;

namespace Paillave.Etl.Core;

public static partial class LeftJoinEx
{
    public static IStream<TOut> LeftJoin<TInLeft, TInRight, TOut, TKey>(this ISortedStream<TInLeft, TKey> leftStream, string name, IKeyedStream<TInRight, TKey> rightStream, Func<TInLeft, TInRight, TOut> resultSelector)
    {
        return new JoinStreamNode<TInLeft, TInRight, TOut, TKey>(name, new JoinArgs<TInLeft, TInRight, TOut, TKey>
        {
            LeftInputStream = leftStream,
            RightInputStream = rightStream,
            ResultSelector = resultSelector,
            RedirectErrorsInsteadOfFail = false
        }).Output;
    }
}
