using System;

namespace Paillave.Etl.Core
{
    public static partial class LookupEx
    {
        public static IStream<Correlated<TOut>> Lookup<TInLeft, TInRight, TOut, TKey>(this IStream<Correlated<TInLeft>> leftStream, string name, IStream<Correlated<TInRight>> rightStream, Func<TInLeft, TKey> leftKey, Func<TInRight, TKey> rightKey, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            return new LookupStreamNode<Correlated<TInLeft>, Correlated<TInRight>, Correlated<TOut>, TKey>(name, new LookupArgs<Correlated<TInLeft>, Correlated<TInRight>, Correlated<TOut>, TKey>
            {
                LeftInputStream = leftStream,
                RightInputStream = rightStream,
                ResultSelector = (cl, r) => new Correlated<TOut> { Row = resultSelector(cl.Row, r == null ? default : r.Row), CorrelationKeys = cl.CorrelationKeys },
                GetLeftStreamKey = cl => leftKey(cl.Row),
                GetRightStreamKey = cr => rightKey(cr.Row)
            }).Output;
        }
        public static IStream<Correlated<TOut>> Lookup<TInLeft, TInRight, TOut, TKey>(this IStream<Correlated<TInLeft>> leftStream, string name, IStream<TInRight> rightStream, Func<TInLeft, TKey> leftKey, Func<TInRight, TKey> rightKey, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            return new LookupStreamNode<Correlated<TInLeft>, TInRight, Correlated<TOut>, TKey>(name, new LookupArgs<Correlated<TInLeft>, TInRight, Correlated<TOut>, TKey>
            {
                LeftInputStream = leftStream,
                RightInputStream = rightStream,
                ResultSelector = (cl, r) => new Correlated<TOut> { Row = resultSelector(cl.Row, r), CorrelationKeys = cl.CorrelationKeys },
                GetLeftStreamKey = cl => leftKey(cl.Row),
                GetRightStreamKey = rightKey
            }).Output;
        }
        public static IStream<TOut> Lookup<TInLeft, TInRight, TOut, TKey>(this IStream<TInLeft> leftStream, string name, IStream<TInRight> rightStream, Func<TInLeft, TKey> leftKey, Func<TInRight, TKey> rightKey, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            return new LookupStreamNode<TInLeft, TInRight, TOut, TKey>(name, new LookupArgs<TInLeft, TInRight, TOut, TKey>
            {
                LeftInputStream = leftStream,
                RightInputStream = rightStream,
                ResultSelector = resultSelector,
                GetLeftStreamKey = leftKey,
                GetRightStreamKey = rightKey
            }).Output;
        }
    }
}
