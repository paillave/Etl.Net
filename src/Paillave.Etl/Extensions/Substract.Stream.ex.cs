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
        public static TStream Substract<TStream, TInLeft, TInRight, TKey>(this TStream leftStream, string name, IStream<TInRight> rightStream, Func<TInLeft, TKey> getLeftKey, Func<TInRight, TKey> getRightKey) where TStream:IStream<TInLeft>
        {
            return new SubstractUnsortedStreamNode<TStream, TInLeft, TInRight, TKey>(name, new SubstractUnsortedArgs<TStream, TInLeft, TInRight, TKey>
            {
                LeftInputStream = leftStream,
                RightInputStream = rightStream,
                GetLeftKey = getLeftKey,
                GetRightKey = getRightKey
            }).Output;
        }
    }
}
