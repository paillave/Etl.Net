using System;

namespace Paillave.Etl.Core
{
    public static partial class SubstractEx
    {
        public static IStream<TInLeft> Substract<TInLeft, TInRight, TKey>(this ISortedStream<TInLeft, TKey> leftStream, string name, ISortedStream<TInRight, TKey> rightStream)
        {
            return new SubstractStreamNode<TInLeft, TInRight, TKey>(name, new SubstractArgs<TInLeft, TInRight, TKey>
            {
                LeftInputStream = leftStream,
                RightInputStream = rightStream,
            }).Output;
        }
        // public static TStream Substract<TStream, TInLeft, TInRight, TKey>(this TStream leftStream, string name, IStream<TInRight> rightStream, Func<TInLeft, TKey> getLeftKey, Func<TInRight, TKey> getRightKey) where TStream : IStream<TInLeft>
        // {
        //     return new SubstractUnsortedStreamNode<TStream, TInLeft, TInRight, TKey>(name, new SubstractUnsortedArgs<TStream, TInLeft, TInRight, TKey>
        //     {
        //         LeftInputStream = leftStream,
        //         RightInputStream = rightStream,
        //         GetLeftKey = getLeftKey,
        //         GetRightKey = getRightKey
        //     }).Output;
        // }
        public static IStream<TInLeft> Substract<TInLeft, TInRight, TKey>(this IStream<TInLeft> leftStream, string name, IStream<TInRight> rightStream, Func<TInLeft, TKey> getLeftKey, Func<TInRight, TKey> getRightKey)
        {
            return new SubstractUnsortedStreamNode<IStream<TInLeft>, TInLeft, TInRight, TKey>(name, new SubstractUnsortedArgs<IStream<TInLeft>, TInLeft, TInRight, TKey>
            {
                LeftInputStream = leftStream,
                RightInputStream = rightStream,
                GetLeftKey = getLeftKey,
                GetRightKey = getRightKey
            }).Output;
        }
    }
}
