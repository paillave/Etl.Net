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
    public static partial class SubProcessEx
    {
        public static IStream<TOut> SubProcess<TIn, TOut>(this IStream<TIn> stream, string name, Func<ISingleStream<TIn>, IStream<TOut>> subProcess, bool noParallelisation = false)
        {
            return new SubProcessStreamNode<TIn, TOut>(name, new SubProcessArgs<TIn, TOut>
            {
                NoParallelisation = noParallelisation,
                SubProcess = subProcess,
                Stream = stream
            }).Output;
        }
    }
}
