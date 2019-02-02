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
}
