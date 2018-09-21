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
        #region Simple process row
        public static IStream<TIn> ToAction<TIn>(this IStream<TIn> stream, string name, Action<TIn> processRow)
        {
            return new ToActionStreamNode<TIn, IStream<TIn>>(name, new ToActionArgs<TIn, IStream<TIn>>
            {
                Processor = new SimpleToActionProcessor<TIn>(processRow),
                Stream = stream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ToAction<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, Action<TIn> processRow)
        {
            return new ToActionStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ToActionArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Processor = new SimpleToActionProcessor<TIn>(processRow),
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ToAction<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, Action<TIn> processRow)
        {
            return new ToActionStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ToActionArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Processor = new SimpleToActionProcessor<TIn>(processRow),
                Stream = stream
            }).Output;
        }
        #endregion

        #region Simple action processor
        public static IStream<TIn> ToAction<TIn>(this IStream<TIn> stream, string name, IToActionProcessor<TIn> processor)
        {
            return new ToActionStreamNode<TIn, IStream<TIn>>(name, new ToActionArgs<TIn, IStream<TIn>>
            {
                Processor = processor,
                Stream = stream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ToAction<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, IToActionProcessor<TIn> processor)
        {
            return new ToActionStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ToActionArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Processor = processor,
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ToAction<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, IToActionProcessor<TIn> processor)
        {
            return new ToActionStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ToActionArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Processor = processor,
                Stream = stream
            }).Output;
        }
        #endregion

        #region Process and preprocess row
        public static IStream<TIn> ToAction<TIn, TResource>(this IStream<TIn> stream, string name, IStream<TResource> resourceStream, Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
        {
            return new ToActionStreamNode<TIn, IStream<TIn>, TResource>(name, new ToActionArgs<TIn, IStream<TIn>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new SimpleToActionProcessor<TIn, TResource>(processRow, preProcess)
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ToAction<TIn, TResource, TKey>(this ISortedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
        {
            return new ToActionStreamNode<TIn, ISortedStream<TIn, TKey>, TResource>(name, new ToActionArgs<TIn, ISortedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new SimpleToActionProcessor<TIn, TResource>(processRow, preProcess)
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ToAction<TIn, TResource, TKey>(this IKeyedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
        {
            return new ToActionStreamNode<TIn, IKeyedStream<TIn, TKey>, TResource>(name, new ToActionArgs<TIn, IKeyedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new SimpleToActionProcessor<TIn, TResource>(processRow, preProcess)
            }).Output;
        }
        #endregion

        #region Process and preprocess processor
        public static IStream<TIn> ToAction<TIn, TResource>(this IStream<TIn> stream, string name, IStream<TResource> resourceStream, IToActionProcessor<TIn, TResource> processor)
        {
            return new ToActionStreamNode<TIn, IStream<TIn>, TResource>(name, new ToActionArgs<TIn, IStream<TIn>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = processor
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ToAction<TIn, TResource, TKey>(this ISortedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, IToActionProcessor<TIn, TResource> processor)
        {
            return new ToActionStreamNode<TIn, ISortedStream<TIn, TKey>, TResource>(name, new ToActionArgs<TIn, ISortedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = processor
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ToAction<TIn, TResource, TKey>(this IKeyedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, IToActionProcessor<TIn, TResource> processor)
        {
            return new ToActionStreamNode<TIn, IKeyedStream<TIn, TKey>, TResource>(name, new ToActionArgs<TIn, IKeyedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = processor
            }).Output;
        }
        #endregion

        #region Simple process row with context
        public static IStream<TIn> ToAction<TIn, TCtx>(this IStream<TIn> stream, string name, TCtx initialContext, Action<TIn, TCtx, Action<TCtx>> processRow)
        {
            return new ToActionStreamNode<TIn, IStream<TIn>>(name, new ToActionArgs<TIn, IStream<TIn>>
            {
                Processor = new ContextToActionProcessor<TIn, TCtx>(processRow, initialContext),
                Stream = stream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ToAction<TIn, TKey, TCtx>(this ISortedStream<TIn, TKey> stream, string name, TCtx initialContext, Action<TIn, TCtx, Action<TCtx>> processRow)
        {
            return new ToActionStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ToActionArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Processor = new ContextToActionProcessor<TIn, TCtx>(processRow, initialContext),
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ToAction<TIn, TKey, TCtx>(this IKeyedStream<TIn, TKey> stream, string name, TCtx initialContext, Action<TIn, TCtx, Action<TCtx>> processRow)
        {
            return new ToActionStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ToActionArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Processor = new ContextToActionProcessor<TIn, TCtx>(processRow, initialContext),
                Stream = stream
            }).Output;
        }
        #endregion

        #region Process and preprocess row with context
        public static IStream<TIn> ToAction<TIn, TResource, TCtx>(this IStream<TIn> stream, string name, IStream<TResource> resourceStream, Action<TIn, TResource, TCtx, Action<TCtx>> processRow, Action<TResource, Action<TCtx>> preProcess = null)
        {
            return new ToActionStreamNode<TIn, IStream<TIn>, TResource>(name, new ToActionArgs<TIn, IStream<TIn>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new ContextToActionProcessor<TIn, TResource, TCtx>(processRow, preProcess)
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ToAction<TIn, TResource, TCtx, TKey>(this ISortedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, Action<TIn, TResource, TCtx, Action<TCtx>> processRow, Action<TResource, Action<TCtx>> preProcess = null)
        {
            return new ToActionStreamNode<TIn, ISortedStream<TIn, TKey>, TResource>(name, new ToActionArgs<TIn, ISortedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new ContextToActionProcessor<TIn, TResource, TCtx>(processRow, preProcess)
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ToAction<TIn, TResource, TCtx, TKey>(this IKeyedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, Action<TIn, TResource, TCtx, Action<TCtx>> processRow, Action<TResource, Action<TCtx>> preProcess = null)
        {
            return new ToActionStreamNode<TIn, IKeyedStream<TIn, TKey>, TResource>(name, new ToActionArgs<TIn, IKeyedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new ContextToActionProcessor<TIn, TResource, TCtx>(processRow, preProcess)
            }).Output;
        }
        #endregion
    }
}
