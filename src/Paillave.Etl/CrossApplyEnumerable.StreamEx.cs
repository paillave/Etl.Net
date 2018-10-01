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
    public static partial class CrossApplyEnumerableEx
    {
        public static IStream<TOut> CrossApplyEnumerable<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<TOut>> values, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new ActionValuesProvider<TIn, TOut>(new ActionValuesProviderArgs<TIn, TOut>()
            {
                NoParallelisation = noParallelisation,
                ProduceValues = (input, push) =>
                {
                    foreach (var value in values(input))
                        push(value);
                }
            }), i => i, (i, _) => i);
        }
        public static IStream<TOut> CrossApplyEnumerable<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> resourceStream, Func<TIn1, TIn2, IEnumerable<TOut>> values, bool noParallelisation = false)
        {
            return stream.CrossApply(name, resourceStream, new ActionResourceValuesProvider<TIn1, TIn2, TOut>(new ActionResourceValuesProviderArgs<TIn1, TIn2, TOut>()
            {
                NoParallelisation = noParallelisation,
                ProduceValues = (input1, input2, push) =>
                {
                    foreach (var value in values(input1, input2))
                        push(value);
                }
            }), (i, _) => i, (i, _, __) => i);
        }
    }
}
