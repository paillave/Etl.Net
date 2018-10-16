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
    public static partial class ThroughActionEx
    {
        #region Simple process row
        public static IStream<TIn> ThroughAction<TIn>(this IStream<TIn> stream, string name, Action<TIn> processRow)
        {
            return new ThroughActionStreamNode<TIn, IStream<TIn>>(name, new ThroughActionArgs<TIn, IStream<TIn>>
            {
                Processor = new SimpleThroughActionProcessor<TIn>(processRow),
                Stream = stream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ThroughAction<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, Action<TIn> processRow)
        {
            return new ThroughActionStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ThroughActionArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Processor = new SimpleThroughActionProcessor<TIn>(processRow),
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ThroughAction<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, Action<TIn> processRow)
        {
            return new ThroughActionStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ThroughActionArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Processor = new SimpleThroughActionProcessor<TIn>(processRow),
                Stream = stream
            }).Output;
        }
        #endregion

        #region Simple action processor
        public static IStream<TIn> ThroughAction<TIn>(this IStream<TIn> stream, string name, IThroughActionProcessor<TIn> processor)
        {
            return new ThroughActionStreamNode<TIn, IStream<TIn>>(name, new ThroughActionArgs<TIn, IStream<TIn>>
            {
                Processor = processor,
                Stream = stream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ThroughAction<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, IThroughActionProcessor<TIn> processor)
        {
            return new ThroughActionStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ThroughActionArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Processor = processor,
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ThroughAction<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, IThroughActionProcessor<TIn> processor)
        {
            return new ThroughActionStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ThroughActionArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Processor = processor,
                Stream = stream
            }).Output;
        }
        #endregion

        #region Process and preprocess row
        public static IStream<TIn> ThroughAction<TIn, TResource>(this IStream<TIn> stream, string name, ISingleStream<TResource> resourceStream, Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
        {
            return new ThroughActionStreamNode<TIn, IStream<TIn>, TResource>(name, new ThroughActionArgs<TIn, IStream<TIn>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new SimpleThroughActionProcessor<TIn, TResource>(processRow, preProcess)
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ThroughAction<TIn, TResource, TKey>(this ISortedStream<TIn, TKey> stream, string name, ISingleStream<TResource> resourceStream, Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
        {
            return new ThroughActionStreamNode<TIn, ISortedStream<TIn, TKey>, TResource>(name, new ThroughActionArgs<TIn, ISortedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new SimpleThroughActionProcessor<TIn, TResource>(processRow, preProcess)
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ThroughAction<TIn, TResource, TKey>(this IKeyedStream<TIn, TKey> stream, string name, ISingleStream<TResource> resourceStream, Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
        {
            return new ThroughActionStreamNode<TIn, IKeyedStream<TIn, TKey>, TResource>(name, new ThroughActionArgs<TIn, IKeyedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new SimpleThroughActionProcessor<TIn, TResource>(processRow, preProcess)
            }).Output;
        }
        #endregion

        #region Process and preprocess processor
        public static IStream<TIn> ThroughAction<TIn, TResource>(this IStream<TIn> stream, string name, ISingleStream<TResource> resourceStream, IThroughActionProcessor<TIn, TResource> processor)
        {
            return new ThroughActionStreamNode<TIn, IStream<TIn>, TResource>(name, new ThroughActionArgs<TIn, IStream<TIn>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = processor
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ThroughAction<TIn, TResource, TKey>(this ISortedStream<TIn, TKey> stream, string name, ISingleStream<TResource> resourceStream, IThroughActionProcessor<TIn, TResource> processor)
        {
            return new ThroughActionStreamNode<TIn, ISortedStream<TIn, TKey>, TResource>(name, new ThroughActionArgs<TIn, ISortedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = processor
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ThroughAction<TIn, TResource, TKey>(this IKeyedStream<TIn, TKey> stream, string name, ISingleStream<TResource> resourceStream, IThroughActionProcessor<TIn, TResource> processor)
        {
            return new ThroughActionStreamNode<TIn, IKeyedStream<TIn, TKey>, TResource>(name, new ThroughActionArgs<TIn, IKeyedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = processor
            }).Output;
        }
        #endregion

        #region Simple process row with context
        public static IStream<TIn> ThroughAction<TIn, TCtx>(this IStream<TIn> stream, string name, TCtx initialContext, Action<TIn, TCtx, Action<TCtx>> processRow)
        {
            return new ThroughActionStreamNode<TIn, IStream<TIn>>(name, new ThroughActionArgs<TIn, IStream<TIn>>
            {
                Processor = new ContextThroughActionProcessor<TIn, TCtx>(processRow, initialContext),
                Stream = stream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ThroughAction<TIn, TKey, TCtx>(this ISortedStream<TIn, TKey> stream, string name, TCtx initialContext, Action<TIn, TCtx, Action<TCtx>> processRow)
        {
            return new ThroughActionStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ThroughActionArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Processor = new ContextThroughActionProcessor<TIn, TCtx>(processRow, initialContext),
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ThroughAction<TIn, TKey, TCtx>(this IKeyedStream<TIn, TKey> stream, string name, TCtx initialContext, Action<TIn, TCtx, Action<TCtx>> processRow)
        {
            return new ThroughActionStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ThroughActionArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Processor = new ContextThroughActionProcessor<TIn, TCtx>(processRow, initialContext),
                Stream = stream
            }).Output;
        }
        #endregion

        #region Process and preprocess row with context
        public static IStream<TIn> ThroughAction<TIn, TResource, TCtx>(this IStream<TIn> stream, string name, ISingleStream<TResource> resourceStream, Action<TIn, TResource, TCtx, Action<TCtx>> processRow, Action<TResource, Action<TCtx>> preProcess = null)
        {
            return new ThroughActionStreamNode<TIn, IStream<TIn>, TResource>(name, new ThroughActionArgs<TIn, IStream<TIn>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new ContextThroughActionProcessor<TIn, TResource, TCtx>(processRow, preProcess)
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ThroughAction<TIn, TResource, TCtx, TKey>(this ISortedStream<TIn, TKey> stream, string name, ISingleStream<TResource> resourceStream, Action<TIn, TResource, TCtx, Action<TCtx>> processRow, Action<TResource, Action<TCtx>> preProcess = null)
        {
            return new ThroughActionStreamNode<TIn, ISortedStream<TIn, TKey>, TResource>(name, new ThroughActionArgs<TIn, ISortedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new ContextThroughActionProcessor<TIn, TResource, TCtx>(processRow, preProcess)
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ThroughAction<TIn, TResource, TCtx, TKey>(this IKeyedStream<TIn, TKey> stream, string name, ISingleStream<TResource> resourceStream, Action<TIn, TResource, TCtx, Action<TCtx>> processRow, Action<TResource, Action<TCtx>> preProcess = null)
        {
            return new ThroughActionStreamNode<TIn, IKeyedStream<TIn, TKey>, TResource>(name, new ThroughActionArgs<TIn, IKeyedStream<TIn, TKey>, TResource>
            {
                Stream = stream,
                ResourceStream = resourceStream,
                Processor = new ContextThroughActionProcessor<TIn, TResource, TCtx>(processRow, preProcess)
            }).Output;
        }
        #endregion
    }
}
