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
    public static partial class CrossApplyActionEx
    {
        public static IStream<TOut> CrossApplyAction<TIn, TOut>(this IStream<TIn> stream, string name, Action<TIn, Action<TOut>> valuesProducer, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new ActionValuesProvider<TIn, TOut>(new ActionValuesProviderArgs<TIn, TOut>()
            {
                NoParallelisation = noParallelisation,
                ProduceValues = valuesProducer
            }), i => i, (i, _) => i);
        }
        public static IStream<TOut> CrossApplyAction<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, ISingleStream<TIn2> resourceStream, Action<TIn1, TIn2, Action<TOut>> valuesProducer, bool noParallelisation = false)
        {
            return stream.CrossApply(name, resourceStream, new ActionResourceValuesProvider<TIn1, TIn2, TOut>(new ActionResourceValuesProviderArgs<TIn1, TIn2, TOut>()
            {
                NoParallelisation = noParallelisation,
                ProduceValues = valuesProducer
            }), (i, _) => i, (i, _, __) => i);
        }
    }
}
