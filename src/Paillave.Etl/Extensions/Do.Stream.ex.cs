using System;

namespace Paillave.Etl.Core
{
    public static partial class ThroughActionEx
    {
        #region Simple process row
        public static IStream<TIn> Do<TIn>(this IStream<TIn> stream, string name, Action<TIn> processRow)
        {
            return new DoStreamNode<TIn, IStream<TIn>>(name, new DoArgs<TIn, IStream<TIn>>
            {
                Processor = new SimpleDoProcessor<TIn, TIn>(i => i, processRow),
                Stream = stream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> Do<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, Action<TIn> processRow)
        {
            return new DoStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new DoArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Processor = new SimpleDoProcessor<TIn, TIn>(i => i, processRow),
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> Do<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, Action<TIn> processRow)
        {
            return new DoStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new DoArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Processor = new SimpleDoProcessor<TIn, TIn>(i => i, processRow),
                Stream = stream
            }).Output;
        }
        public static ISingleStream<TIn> Do<TIn>(this ISingleStream<TIn> stream, string name, Action<TIn> processRow)
        {
            return new DoStreamNode<TIn, ISingleStream<TIn>>(name, new DoArgs<TIn, ISingleStream<TIn>>
            {
                Processor = new SimpleDoProcessor<TIn, TIn>(i => i, processRow),
                Stream = stream
            }).Output;
        }
        public static IStream<Correlated<TIn>> DoCorrelated<TIn>(this IStream<Correlated<TIn>> stream, string name, Action<TIn> processRow)
        {
            return new DoStreamNode<Correlated<TIn>, IStream<Correlated<TIn>>>(name, new DoArgs<Correlated<TIn>, IStream<Correlated<TIn>>>
            {
                Processor = new SimpleDoProcessor<Correlated<TIn>, TIn>(i => i.Row, processRow),
                Stream = stream
            }).Output;
        }
        public static ISortedStream<Correlated<TIn>, TKey> DoCorrelated<TIn, TKey>(this ISortedStream<Correlated<TIn>, TKey> stream, string name, Action<TIn> processRow)
        {
            return new DoStreamNode<Correlated<TIn>, ISortedStream<Correlated<TIn>, TKey>>(name, new DoArgs<Correlated<TIn>, ISortedStream<Correlated<TIn>, TKey>>
            {
                Processor = new SimpleDoProcessor<Correlated<TIn>, TIn>(i => i.Row, processRow),
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<Correlated<TIn>, TKey> DoCorrelated<TIn, TKey>(this IKeyedStream<Correlated<TIn>, TKey> stream, string name, Action<TIn> processRow)
        {
            return new DoStreamNode<Correlated<TIn>, IKeyedStream<Correlated<TIn>, TKey>>(name, new DoArgs<Correlated<TIn>, IKeyedStream<Correlated<TIn>, TKey>>
            {
                Processor = new SimpleDoProcessor<Correlated<TIn>, TIn>(i => i.Row, processRow),
                Stream = stream
            }).Output;
        }
        public static ISingleStream<Correlated<TIn>> DoCorrelated<TIn>(this ISingleStream<Correlated<TIn>> stream, string name, Action<TIn> processRow)
        {
            return new DoStreamNode<Correlated<TIn>, ISingleStream<Correlated<TIn>>>(name, new DoArgs<Correlated<TIn>, ISingleStream<Correlated<TIn>>>
            {
                Processor = new SimpleDoProcessor<Correlated<TIn>, TIn>(i => i.Row, processRow),
                Stream = stream
            }).Output;
        }
        #endregion



        #region Process row with injection
        public static IStream<TIn> Do<TIn>(this IStream<TIn> stream, string name, Func<DoWithResolutionProcessorBuilder<TIn, TIn>, IDoProcessor<TIn>> o)
        {
            return new DoStreamNode<TIn, IStream<TIn>>(name, new DoArgs<TIn, IStream<TIn>>
            {
                Processor = o(new DoWithResolutionProcessorBuilder<TIn, TIn>(i => i)),
                Stream = stream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> Do<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, Func<DoWithResolutionProcessorBuilder<TIn, TIn>, IDoProcessor<TIn>> o)
        {
            return new DoStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new DoArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Processor = o(new DoWithResolutionProcessorBuilder<TIn, TIn>(i => i)),
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> Do<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, Func<DoWithResolutionProcessorBuilder<TIn, TIn>, IDoProcessor<TIn>> o)
        {
            return new DoStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new DoArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Processor = o(new DoWithResolutionProcessorBuilder<TIn, TIn>(i => i)),
                Stream = stream
            }).Output;
        }
        public static ISingleStream<TIn> Do<TIn>(this ISingleStream<TIn> stream, string name, Func<DoWithResolutionProcessorBuilder<TIn, TIn>, IDoProcessor<TIn>> o)
        {
            return new DoStreamNode<TIn, ISingleStream<TIn>>(name, new DoArgs<TIn, ISingleStream<TIn>>
            {
                Processor = o(new DoWithResolutionProcessorBuilder<TIn, TIn>(i => i)),
                Stream = stream
            }).Output;
        }
        public static IStream<Correlated<TIn>> DoCorrelated<TIn>(this IStream<Correlated<TIn>> stream, string name, Func<DoWithResolutionProcessorBuilder<Correlated<TIn>, TIn>, IDoProcessor<Correlated<TIn>>> o)
        {
            return new DoStreamNode<Correlated<TIn>, IStream<Correlated<TIn>>>(name, new DoArgs<Correlated<TIn>, IStream<Correlated<TIn>>>
            {
                Processor = o(new DoWithResolutionProcessorBuilder<Correlated<TIn>, TIn>(i => i.Row)),
                Stream = stream
            }).Output;
        }
        public static ISortedStream<Correlated<TIn>, TKey> DoCorrelated<TIn, TKey>(this ISortedStream<Correlated<TIn>, TKey> stream, string name, Func<DoWithResolutionProcessorBuilder<Correlated<TIn>, TIn>, IDoProcessor<Correlated<TIn>>> o)
        {
            return new DoStreamNode<Correlated<TIn>, ISortedStream<Correlated<TIn>, TKey>>(name, new DoArgs<Correlated<TIn>, ISortedStream<Correlated<TIn>, TKey>>
            {
                Processor = o(new DoWithResolutionProcessorBuilder<Correlated<TIn>, TIn>(i => i.Row)),
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<Correlated<TIn>, TKey> DoCorrelated<TIn, TKey>(this IKeyedStream<Correlated<TIn>, TKey> stream, string name, Func<DoWithResolutionProcessorBuilder<Correlated<TIn>, TIn>, IDoProcessor<Correlated<TIn>>> o)
        {
            return new DoStreamNode<Correlated<TIn>, IKeyedStream<Correlated<TIn>, TKey>>(name, new DoArgs<Correlated<TIn>, IKeyedStream<Correlated<TIn>, TKey>>
            {
                Processor = o(new DoWithResolutionProcessorBuilder<Correlated<TIn>, TIn>(i => i.Row)),
                Stream = stream
            }).Output;
        }
        public static ISingleStream<Correlated<TIn>> DoCorrelated<TIn>(this ISingleStream<Correlated<TIn>> stream, string name, Func<DoWithResolutionProcessorBuilder<Correlated<TIn>, TIn>, IDoProcessor<Correlated<TIn>>> o)
        {
            return new DoStreamNode<Correlated<TIn>, ISingleStream<Correlated<TIn>>>(name, new DoArgs<Correlated<TIn>, ISingleStream<Correlated<TIn>>>
            {
                Processor = o(new DoWithResolutionProcessorBuilder<Correlated<TIn>, TIn>(i => i.Row)),
                Stream = stream
            }).Output;
        }
        #endregion




        #region Simple action processor
        public static IStream<TIn> Do<TIn>(this IStream<TIn> stream, string name, IDoProcessor<TIn> processor)
        {
            return new DoStreamNode<TIn, IStream<TIn>>(name, new DoArgs<TIn, IStream<TIn>>
            {
                Processor = processor,
                Stream = stream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> Do<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, IDoProcessor<TIn> processor)
        {
            return new DoStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new DoArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Processor = processor,
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> Do<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, IDoProcessor<TIn> processor)
        {
            return new DoStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new DoArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Processor = processor,
                Stream = stream
            }).Output;
        }
        #endregion
    }
}
