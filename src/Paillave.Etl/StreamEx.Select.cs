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
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> mapper, bool excludeNull = false)
        {
            return new SelectStreamNode<TIn, TOut>(name, new SelectArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = new SimpleSelectProcessor<TIn, TOut>(mapper),
                ExcludeNull = excludeNull
            }).Output;
        }
        public static IStream<TOut> Select<TIn, TOut, TCtx>(this IStream<TIn> stream, string name, TCtx initialContext, Func<TIn, TCtx, Action<TCtx>, TOut> mapper, bool excludeNull = false)
        {
            return new SelectStreamNode<TIn, TOut>(name, new SelectArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = new ContextSelectProcessor<TIn, TOut, TCtx>(mapper, initialContext),
                ExcludeNull = excludeNull
            }).Output;
        }
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, ISelectProcessor<TIn, TOut> processor, bool excludeNull = false)
        {
            return new SelectStreamNode<TIn, TOut>(name, new SelectArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = processor,
                ExcludeNull = excludeNull
            }).Output;
        }
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, ISelectWithIndexProcessor<TIn, TOut> processor, bool excludeNull = false)
        {
            return new SelectWithIndexStreamNode<TIn, TOut>(name, new SelectWithIndexArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = processor,
                ExcludeNull = excludeNull
            }).Output;
        }
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, int, TOut> mapper, bool excludeNull = false)
        {
            return new SelectWithIndexStreamNode<TIn, TOut>(name, new SelectWithIndexArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = new SimpleSelectWithIndexProcessor<TIn, TOut>(mapper),
                ExcludeNull = excludeNull
            }).Output;
        }
        public static IStream<TOut> Select<TIn, TOut, TCtx>(this IStream<TIn> stream, string name, TCtx initialContext, Func<TIn, int, TCtx, Action<TCtx>, TOut> mapper, bool excludeNull = false)
        {
            return new SelectWithIndexStreamNode<TIn, TOut>(name, new SelectWithIndexArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = new ContextSelectWithIndexProcessor<TIn, TOut, TCtx>(mapper, initialContext),
                ExcludeNull = excludeNull
            }).Output;
        }
    }
}
