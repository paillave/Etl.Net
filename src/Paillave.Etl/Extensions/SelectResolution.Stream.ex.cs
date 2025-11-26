using System;

namespace Paillave.Etl.Core;

/// <summary>
/// Set of extensions to transform elements of the input stream into another element.
/// </summary>
/// <remarks>
/// If TOut implements <c>IDisposable</c> it will be automatically disposed once the ETL process is finished.
/// </remarks>
public static partial class SelectResolutionEx
{
    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream</param>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<TOut> SelectResolved<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectStreamNode<TIn, TOut>(name, new SelectArgs<TIn, TOut>
        {
            Stream = stream,
            Processor = new SimpleSelectProcessor<TIn, TOut>(resultSelector),
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static IStream<TOut> SelectResolved<TIn, TKey, TOut>(this IStream<TIn> stream, string name, Func<TIn, TKey> sequenceGroupKeySelector, Func<TIn, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectStreamNode<TIn, TOut>(name, new SelectArgs<TIn, TOut>
        {
            Stream = stream,
            Processor = new SelectWithSequenceProcessor<TIn, TKey, TOut>(resultSelector, sequenceGroupKeySelector),
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static ISingleStream<TOut> SelectResolved<TIn, TOut>(this ISingleStream<TIn> stream, string name, Func<TIn, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectSingleStreamNode<TIn, TOut>(name, new SelectSingleArgs<TIn, TOut>
        {
            Stream = stream,
            Processor = new SimpleSelectProcessor<TIn, TOut>(resultSelector),
            WithNoDispose = withNoDispose
        }).Output;
    }
    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression based on the occurrence index
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream using an occurrence index</param>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<TOut> SelectResolved<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectWithIndexStreamNode<TIn, TOut>(name, new SelectWithIndexArgs<TIn, TOut>
        {
            Stream = stream,
            Processor = new SimpleSelectWithIndexProcessor<TIn, TOut>(resultSelector),
            WithNoDispose = withNoDispose
        }).Output;
    }
    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression. 
    /// The calculation takes in consideration a single element stream
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="streamToApply">The stream that contains the single element that will be applied to each element of the main stream with the result selector</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream using the unique element of the stream to apply</param>
    /// <typeparam name="TIn1">Main stream type</typeparam>
    /// <typeparam name="TIn2">Applied stream type</typeparam>
    /// <typeparam name="TOut">Output stream</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<TOut> SelectResolved<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplyStreamNode<TIn1, TIn2, TOut>(name, new ApplyArgs<TIn1, TIn2, TOut>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            IndexSelector = resultSelector,
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static ISingleStream<TOut> SelectResolved<TIn1, TIn2, TOut>(this ISingleStream<TIn1> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplySingleStreamNode<TIn1, TIn2, TOut>(name, new ApplySingleArgs<TIn1, TIn2, TOut>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            IndexSelector = resultSelector,
            WithNoDispose = withNoDispose
        }).Output;
    }
    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression and the occurrence index. 
    /// The calculation takes in consideration a single element stream
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="streamToApply">The stream that contains the single element that will be applied to each element of the main stream with the result selector</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream using an occurrence index</param>
    /// <typeparam name="TIn1">Main stream type</typeparam>
    /// <typeparam name="TIn2">Applied stream type</typeparam>
    /// <typeparam name="TOut">Output stream</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<TOut> SelectResolved<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplyStreamNode<TIn1, TIn2, TOut>(name, new ApplyArgs<TIn1, TIn2, TOut>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            Selector = resultSelector,
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static ISingleStream<TOut> SelectResolved<TIn1, TIn2, TOut>(this ISingleStream<TIn1> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplySingleStreamNode<TIn1, TIn2, TOut>(name, new ApplySingleArgs<TIn1, TIn2, TOut>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            Selector = resultSelector,
            WithNoDispose = withNoDispose
        }).Output;
    }

























    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream</param>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<Correlated<TOut>> SelectResolved<TIn, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectStreamNode<Correlated<TIn>, Correlated<TOut>>(name, new SelectArgs<Correlated<TIn>, Correlated<TOut>>
        {
            Stream = stream,
            Processor = new SimpleSelectProcessor<Correlated<TIn>, Correlated<TOut>>((i, services) => new Correlated<TOut> { Row = resultSelector(i.Row, services), CorrelationKeys = i.CorrelationKeys }),
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static IStream<Correlated<TOut>> SelectResolved<TIn, TKey, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKey> sequenceGroupKeySelector, Func<TIn, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectStreamNode<Correlated<TIn>, Correlated<TOut>>(name, new SelectArgs<Correlated<TIn>, Correlated<TOut>>
        {
            Stream = stream,
            Processor = new SelectWithSequenceProcessor<Correlated<TIn>, TKey, Correlated<TOut>>((i, idx, services) => new Correlated<TOut> { Row = resultSelector(i.Row, idx, services) }, i => sequenceGroupKeySelector(i.Row)),
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static ISingleStream<Correlated<TOut>> SelectResolved<TIn, TOut>(this ISingleStream<Correlated<TIn>> stream, string name, Func<TIn, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectSingleStreamNode<Correlated<TIn>, Correlated<TOut>>(name, new SelectSingleArgs<Correlated<TIn>, Correlated<TOut>>
        {
            Stream = stream,
            Processor = new SimpleSelectProcessor<Correlated<TIn>, Correlated<TOut>>((i, services) => new Correlated<TOut> { Row = resultSelector(i.Row, services), CorrelationKeys = i.CorrelationKeys }),
            WithNoDispose = withNoDispose
        }).Output;
    }
    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression based on the occurrence index
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream using an occurrence index</param>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<Correlated<TOut>> SelectResolved<TIn, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectWithIndexStreamNode<Correlated<TIn>, Correlated<TOut>>(name, new SelectWithIndexArgs<Correlated<TIn>, Correlated<TOut>>
        {
            Stream = stream,
            Processor = new SimpleSelectWithIndexProcessor<Correlated<TIn>, Correlated<TOut>>((i, idx, services) => new Correlated<TOut> { Row = resultSelector(i.Row, idx, services), CorrelationKeys = i.CorrelationKeys }),
            WithNoDispose = withNoDispose
        }).Output;
    }
    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression. 
    /// The calculation takes in consideration a single element stream
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="streamToApply">The stream that contains the single element that will be applied to each element of the main stream with the result selector</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream using the unique element of the stream to apply</param>
    /// <typeparam name="TIn1">Main stream type</typeparam>
    /// <typeparam name="TIn2">Applied stream type</typeparam>
    /// <typeparam name="TOut">Output stream</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<Correlated<TOut>> SelectResolved<TIn1, TIn2, TOut>(this IStream<Correlated<TIn1>> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplyStreamNode<Correlated<TIn1>, TIn2, Correlated<TOut>>(name, new ApplyArgs<Correlated<TIn1>, TIn2, Correlated<TOut>>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            IndexSelector = (i1, i2, idx, services) => new Correlated<TOut> { Row = resultSelector(i1.Row, i2, idx, services), CorrelationKeys = i1.CorrelationKeys },
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static ISingleStream<Correlated<TOut>> SelectResolved<TIn1, TIn2, TOut>(this ISingleStream<Correlated<TIn1>> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplySingleStreamNode<Correlated<TIn1>, TIn2, Correlated<TOut>>(name, new ApplySingleArgs<Correlated<TIn1>, TIn2, Correlated<TOut>>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            IndexSelector = (i1, i2, idx, services) => new Correlated<TOut> { Row = resultSelector(i1.Row, i2, idx, services), CorrelationKeys = i1.CorrelationKeys },
            WithNoDispose = withNoDispose
        }).Output;
    }
    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression and the occurrence index. 
    /// The calculation takes in consideration a single element stream
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="streamToApply">The stream that contains the single element that will be applied to each element of the main stream with the result selector</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream using an occurrence index</param>
    /// <typeparam name="TIn1">Main stream type</typeparam>
    /// <typeparam name="TIn2">Applied stream type</typeparam>
    /// <typeparam name="TOut">Output stream</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<Correlated<TOut>> SelectResolved<TIn1, TIn2, TOut>(this IStream<Correlated<TIn1>> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplyStreamNode<Correlated<TIn1>, TIn2, Correlated<TOut>>(name, new ApplyArgs<Correlated<TIn1>, TIn2, Correlated<TOut>>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            Selector = (i1, i2, services) => new Correlated<TOut> { Row = resultSelector(i1.Row, i2, services), CorrelationKeys = i1.CorrelationKeys },
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static ISingleStream<Correlated<TOut>> SelectResolved<TIn1, TIn2, TOut>(this ISingleStream<Correlated<TIn1>> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplySingleStreamNode<Correlated<TIn1>, TIn2, Correlated<TOut>>(name, new ApplySingleArgs<Correlated<TIn1>, TIn2, Correlated<TOut>>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            Selector = (i1, i2, services) => new Correlated<TOut> { Row = resultSelector(i1.Row, i2, services), CorrelationKeys = i1.CorrelationKeys },
            WithNoDispose = withNoDispose
        }).Output;
    }







































    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream</param>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<Correlated<TOut>> SelectCorrelated<TIn, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectStreamNode<Correlated<TIn>, Correlated<TOut>>(name, new SelectArgs<Correlated<TIn>, Correlated<TOut>>
        {
            Stream = stream,
            Processor = new SimpleSelectProcessor<Correlated<TIn>, Correlated<TOut>>((i, services) => new Correlated<TOut> { Row = resultSelector(i.Row, services), CorrelationKeys = i.CorrelationKeys }),
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static IStream<Correlated<TOut>> SelectCorrelated<TIn, TKey, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKey> sequenceGroupKeySelector, Func<TIn, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectStreamNode<Correlated<TIn>, Correlated<TOut>>(name, new SelectArgs<Correlated<TIn>, Correlated<TOut>>
        {
            Stream = stream,
            Processor = new SelectWithSequenceProcessor<Correlated<TIn>, TKey, Correlated<TOut>>((i, idx, services) => new Correlated<TOut> { Row = resultSelector(i.Row, idx, services) }, i => sequenceGroupKeySelector(i.Row)),
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static ISingleStream<Correlated<TOut>> SelectCorrelated<TIn, TOut>(this ISingleStream<Correlated<TIn>> stream, string name, Func<TIn, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectSingleStreamNode<Correlated<TIn>, Correlated<TOut>>(name, new SelectSingleArgs<Correlated<TIn>, Correlated<TOut>>
        {
            Stream = stream,
            Processor = new SimpleSelectProcessor<Correlated<TIn>, Correlated<TOut>>((i, services) => new Correlated<TOut> { Row = resultSelector(i.Row, services), CorrelationKeys = i.CorrelationKeys }),
            WithNoDispose = withNoDispose
        }).Output;
    }
    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression based on the occurrence index
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream using an occurrence index</param>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<Correlated<TOut>> SelectCorrelated<TIn, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new SelectWithIndexStreamNode<Correlated<TIn>, Correlated<TOut>>(name, new SelectWithIndexArgs<Correlated<TIn>, Correlated<TOut>>
        {
            Stream = stream,
            Processor = new SimpleSelectWithIndexProcessor<Correlated<TIn>, Correlated<TOut>>((i, idx, services) => new Correlated<TOut> { Row = resultSelector(i.Row, idx, services), CorrelationKeys = i.CorrelationKeys }),
            WithNoDispose = withNoDispose
        }).Output;
    }
    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression. 
    /// The calculation takes in consideration a single element stream
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="streamToApply">The stream that contains the single element that will be applied to each element of the main stream with the result selector</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream using the unique element of the stream to apply</param>
    /// <typeparam name="TIn1">Main stream type</typeparam>
    /// <typeparam name="TIn2">Applied stream type</typeparam>
    /// <typeparam name="TOut">Output stream</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<Correlated<TOut>> SelectCorrelated<TIn1, TIn2, TOut>(this IStream<Correlated<TIn1>> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplyStreamNode<Correlated<TIn1>, TIn2, Correlated<TOut>>(name, new ApplyArgs<Correlated<TIn1>, TIn2, Correlated<TOut>>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            IndexSelector = (i1, i2, idx, services) => new Correlated<TOut> { Row = resultSelector(i1.Row, i2, idx, services), CorrelationKeys = i1.CorrelationKeys },
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static ISingleStream<Correlated<TOut>> SelectCorrelated<TIn1, TIn2, TOut>(this ISingleStream<Correlated<TIn1>> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, int, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplySingleStreamNode<Correlated<TIn1>, TIn2, Correlated<TOut>>(name, new ApplySingleArgs<Correlated<TIn1>, TIn2, Correlated<TOut>>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            IndexSelector = (i1, i2, idx, services) => new Correlated<TOut> { Row = resultSelector(i1.Row, i2, idx, services), CorrelationKeys = i1.CorrelationKeys },
            WithNoDispose = withNoDispose
        }).Output;
    }
    /// <summary>
    /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression and the occurrence index. 
    /// The calculation takes in consideration a single element stream
    /// </summary>
    /// <param name="stream">Input stream</param>
    /// <param name="name">Name of the operation</param>
    /// <param name="streamToApply">The stream that contains the single element that will be applied to each element of the main stream with the result selector</param>
    /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream using an occurrence index</param>
    /// <typeparam name="TIn1">Main stream type</typeparam>
    /// <typeparam name="TIn2">Applied stream type</typeparam>
    /// <typeparam name="TOut">Output stream</typeparam>
    /// <returns>Output stream</returns>
    public static IStream<Correlated<TOut>> SelectCorrelated<TIn1, TIn2, TOut>(this IStream<Correlated<TIn1>> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplyStreamNode<Correlated<TIn1>, TIn2, Correlated<TOut>>(name, new ApplyArgs<Correlated<TIn1>, TIn2, Correlated<TOut>>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            Selector = (i1, i2, services) => new Correlated<TOut> { Row = resultSelector(i1.Row, i2, services), CorrelationKeys = i1.CorrelationKeys },
            WithNoDispose = withNoDispose
        }).Output;
    }
    public static ISingleStream<Correlated<TOut>> SelectCorrelated<TIn1, TIn2, TOut>(this ISingleStream<Correlated<TIn1>> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, IServiceProvider, TOut> resultSelector, bool withNoDispose = false)
    {
        return new ApplySingleStreamNode<Correlated<TIn1>, TIn2, Correlated<TOut>>(name, new ApplySingleArgs<Correlated<TIn1>, TIn2, Correlated<TOut>>
        {
            MainStream = stream,
            StreamToApply = streamToApply,
            Selector = (i1, i2, services) => new Correlated<TOut> { Row = resultSelector(i1.Row, i2, services), CorrelationKeys = i1.CorrelationKeys },
            WithNoDispose = withNoDispose
        }).Output;
    }
}
