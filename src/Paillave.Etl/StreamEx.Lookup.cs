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

namespace Paillave.Etl
{
    public static partial class StreamEx
    {
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
