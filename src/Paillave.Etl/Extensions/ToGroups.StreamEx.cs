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
    public static partial class ToGroupEx
    {
        public static IStream<TOut> ToGroups<TIn, TKey, TOut>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, Func<IStream<TIn>, IStream<TOut>> subProcess)
        {
            return new ToGroupsStreamNode<TIn, TKey, TOut>(name, new ToGroupsArgs<TIn, TKey, TOut>
            {
                SubProcess = subProcess,
                Stream = stream,
                GetKey = getKey
            }).Output;
        }
    }
}
