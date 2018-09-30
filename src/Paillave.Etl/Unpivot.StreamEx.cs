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
    public static partial class UnpivotEx
    {
        public static IStream<TOut> Unpivot<TIn, TUnpivoted, TOut>(this IStream<TIn> stream, string name, IEnumerable<Func<TIn, TUnpivoted>> fieldsToUnpivot, Func<TIn, TUnpivoted, TOut> resultSelector)
        {
            return new UnpivotStreamNode<TIn, TUnpivoted, TOut>(name, new UnpivotArgs<TIn, TUnpivoted, TOut>
            {
                InputStream = stream,
                FieldsToUnpivot = fieldsToUnpivot,
                ResultSelector = resultSelector
            }).Output;
        }
    }
}
