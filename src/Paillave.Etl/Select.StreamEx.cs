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
    /// <summary>
    /// Set of extensions to transform elements of the input stream into another element.
    /// </summary>
    /// <remarks>
    /// If TOut implements <c>IDisposable</c> it will be automatically disposed once the ETL process is finished.
    /// </remarks>
    public static partial class SelectEx
    {
        /// <summary>
        /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream</param>
        /// <param name="excludeNull">Any output that is null won't be issued</param>
        /// <typeparam name="TIn">Input type</typeparam>
        /// <typeparam name="TOut">Output type</typeparam>
        /// <returns>Output stream</returns>
        /// <example>
        /// This example creates a file path from the input value and open it for writing.
        /// <code>
        /// public class MyJob : IStreamProcessDefinition&lt;string&gt;
        /// {
        ///     public string Name => "example select";
        ///     public void DefineProcess(IStream&lt;string&gt; rootStream)
        ///     {
        ///         rootStream
        ///             .Select("get file path from its name", fileName => Path.Combine("C:\", fileName))
        ///             // Values issued by the following select will be automatically disposed once the process is completed.
        ///             .Select("open the file", path => File.OpenWrite(path)); 
        ///     }
        /// }
        /// </code>
        /// </example>
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> resultSelector, bool excludeNull = false)
        {
            return new SelectStreamNode<TIn, TOut>(name, new SelectArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = new SimpleSelectProcessor<TIn, TOut>(resultSelector),
                ExcludeNull = excludeNull
            }).Output;
        }
        /// <summary>
        /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression that works with a context
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="initialContext">Value of the initial context</param>
        /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream using a context by permiting to change it</param>
        /// <param name="excludeNull">Any output that is null won't be issued</param>
        /// <typeparam name="TIn">Input type</typeparam>
        /// <typeparam name="TOut">Output type</typeparam>
        /// <typeparam name="TCtx">Context type</typeparam>
        /// <returns>Output stream</returns>
        /// <example>
        /// This example returns a dataset by replacing null values with the latest not null value.
        /// <code>
        /// public class CrossApplyActionJobs : IStreamProcessDefinition&lt;object&gt;
        /// {
        ///     public string Name => "import file";
        ///     public void DefineProcess(IStream&lt;object&gt; rootStream)
        ///     {
        ///         rootStream
        ///             .CrossApplyEnumerable("create some values", (input) => Enumerable.Range(0, 10).Select(i => new { Id = i, Value = (i % 3 == 0) ? i : (int?)null }))
        ///             .Select("set null value to the previous not null value", 0, (i, ctx, setCtx) =>
        ///             {
        ///                 var v = i.Value ?? ctx;
        ///                 setCtx(v);
        ///                 return new { i.Id, Value = v };
        ///             });
        ///     }
        /// }
        /// </code>
        /// </example>

        public static IStream<TOut> Select<TIn, TOut, TCtx>(this IStream<TIn> stream, string name, TCtx initialContext, Func<TIn, TCtx, Action<TCtx>, TOut> resultSelector, bool excludeNull = false)
        {
            return new SelectStreamNode<TIn, TOut>(name, new SelectArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = new ContextSelectProcessor<TIn, TOut, TCtx>(resultSelector, initialContext),
                ExcludeNull = excludeNull
            }).Output;
        }
        /// <summary>
        /// Transform the input stream into a similar stream but with a different structure and calculation using a processor
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="processor">Processor that will handle the transformation</param>
        /// <param name="excludeNull">Any output that is null won't be issued</param>
        /// <typeparam name="TIn">Input type</typeparam>
        /// <typeparam name="TOut">Output type</typeparam>
        /// <returns>Output stream</returns>
        /// <example>
        /// This example returns a dataset by replacing null values with the latest not null value.
        /// <code>
        /// public class CrossApplyActionJobs2 : IStreamProcessDefinition&lt;object&gt;
        /// {
        ///     public string Name => "import file";
        ///     public void DefineProcess(IStream&lt;object&gt; rootStream)
        ///     {
        ///         rootStream
        ///             .CrossApplyEnumerable("create some values", (input) => Enumerable.Range(0, 10).Select(i => new MyInputType { Id = i, Value = (i % 3 == 0) ? i : (int?)null }))
        ///             .Select("set null value to the previous not null value", new MySelectProcessor());
        ///     }
        ///     private class MyInputType
        ///     {
        ///         public int Id { get; set; }
        ///         public int? Value { get; set; }
        ///     }
        ///     private class MyOutputType
        ///     {
        ///         public int Id { get; set; }
        ///         public int Value { get; set; }
        ///     }
        ///     private class MySelectProcessor : ISelectProcessor&lt;MyInputType, MyOutputType&gt;
        ///     {
        ///         private int _lastValue = 0;
        ///         public MyOutputType ProcessRow(MyInputType value)
        ///         {
        ///             if (value.Value != null) _lastValue = value.Value.Value;
        ///             return new MyOutputType
        ///             {
        ///                 Id = value.Id,
        ///                 Value = _lastValue
        ///             };
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, ISelectProcessor<TIn, TOut> processor, bool excludeNull = false)
        {
            return new SelectStreamNode<TIn, TOut>(name, new SelectArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = processor,
                ExcludeNull = excludeNull
            }).Output;
        }
        /// <summary>
        /// Transform the input stream into a similar stream but with a different structure and calculation using a processor based on the occurrence index
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="processor">Processor that will handle the transformation using the occurrence index</param>
        /// <param name="excludeNull">Any output that is null won't be issued</param>
        /// <typeparam name="TIn">Input type</typeparam>
        /// <typeparam name="TOut">Output type</typeparam>
        /// <returns>Output stream</returns>
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, ISelectWithIndexProcessor<TIn, TOut> processor, bool excludeNull = false)
        {
            return new SelectWithIndexStreamNode<TIn, TOut>(name, new SelectWithIndexArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = processor,
                ExcludeNull = excludeNull
            }).Output;
        }
        /// <summary>
        /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression based on the occurrence index
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream using an occurrence index</param>
        /// <param name="excludeNull">Any output that is null won't be issued</param>
        /// <typeparam name="TIn">Input type</typeparam>
        /// <typeparam name="TOut">Output type</typeparam>
        /// <returns>Output stream</returns>
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, int, TOut> resultSelector, bool excludeNull = false)
        {
            return new SelectWithIndexStreamNode<TIn, TOut>(name, new SelectWithIndexArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = new SimpleSelectWithIndexProcessor<TIn, TOut>(resultSelector),
                ExcludeNull = excludeNull
            }).Output;
        }
        /// <summary>
        /// Transform the input stream into a similar stream but with a different structure and calculation using a lambda expression that works with a context and the occurrence index
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="initialContext">Value of the initial context</param>
        /// <param name="resultSelector">Transformation to apply on an occurrence of an element of the stream with a context and the occurrence index</param>
        /// <param name="excludeNull">Any output that is null won't be issued</param>
        /// <typeparam name="TIn">Input type</typeparam>
        /// <typeparam name="TOut">Output type</typeparam>
        /// <typeparam name="TCtx">Context type</typeparam>
        /// <returns>Output stream</returns>
        public static IStream<TOut> Select<TIn, TOut, TCtx>(this IStream<TIn> stream, string name, TCtx initialContext, Func<TIn, int, TCtx, Action<TCtx>, TOut> resultSelector, bool excludeNull = false)
        {
            return new SelectWithIndexStreamNode<TIn, TOut>(name, new SelectWithIndexArgs<TIn, TOut>
            {
                Stream = stream,
                Processor = new ContextSelectWithIndexProcessor<TIn, TOut, TCtx>(resultSelector, initialContext),
                ExcludeNull = excludeNull
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
        /// <param name="excludeNull">Any output that is null won't be issued</param>
        /// <typeparam name="TIn1">Main stream type</typeparam>
        /// <typeparam name="TIn2">Applied stream type</typeparam>
        /// <typeparam name="TOut">Output stream</typeparam>
        /// <returns>Output stream</returns>
        public static IStream<TOut> Select<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> streamToApply, Func<TIn1, TIn2, int, TOut> resultSelector, bool excludeNull = false)
        {
            return new ApplyStreamNode<TIn1, TIn2, TOut>(name, new ApplyArgs<TIn1, TIn2, TOut>
            {
                MainStream = stream,
                StreamToApply = streamToApply,
                IndexSelector = resultSelector,
                ExcludeNull = excludeNull
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
        /// <param name="excludeNull">Any output that is null won't be issued</param>
        /// <typeparam name="TIn1">Main stream type</typeparam>
        /// <typeparam name="TIn2">Applied stream type</typeparam>
        /// <typeparam name="TOut">Output stream</typeparam>
        /// <returns>Output stream</returns>
        public static IStream<TOut> Select<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> streamToApply, Func<TIn1, TIn2, TOut> resultSelector, bool excludeNull = false)
        {
            return new ApplyStreamNode<TIn1, TIn2, TOut>(name, new ApplyArgs<TIn1, TIn2, TOut>
            {
                MainStream = stream,
                StreamToApply = streamToApply,
                Selector = resultSelector,
                ExcludeNull = excludeNull
            }).Output;
        }
    }
}
