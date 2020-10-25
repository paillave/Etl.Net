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
}
