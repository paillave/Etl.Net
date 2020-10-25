using Paillave.Etl.Core.Streams;
using System;
using Paillave.Etl.Core;

namespace Paillave.Etl.EntityFrameworkCore
{
    public static class EntityFrameworkCoreSaveEx
    {
        public static IStream<TOut> EfCoreSave<TIn, TInEf, TOut>(
            this IStream<TIn> stream,
            string name,
            Func<EfCoreSaveArgsBuilder<TIn, TIn, TIn>, EfCoreSaveArgsBuilder<TInEf, TIn, TOut>> getOptions)
                where TInEf : class
                    => new EfCoreSaveStreamNode<TInEf, TIn, TOut>(name, getOptions(new EfCoreSaveArgsBuilder<TIn, TIn, TIn>(stream, i => i, (i, j) => i)).Args).Output;

        public static IStream<Correlated<TOut>> EfCoreSave<TIn, TInEf, TOut>(
            this IStream<Correlated<TIn>> stream,
            string name,
            Func<EfCoreSaveCorrelatedArgsBuilder<TIn, TIn, TIn>, EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TOut>> getOptions)
                where TInEf : class
                    => new EfCoreSaveStreamNode<TInEf, Correlated<TIn>, Correlated<TOut>>(name, getOptions(new EfCoreSaveCorrelatedArgsBuilder<TIn, TIn, TIn>(stream, i => i, (i, j) => i)).Args).Output;
        // copy/paste to explicitly solve ambiguity if any
        public static IStream<Correlated<TOut>> EfCoreSaveCorrelated<TIn, TInEf, TOut>(
            this IStream<Correlated<TIn>> stream,
            string name,
            Func<EfCoreSaveCorrelatedArgsBuilder<TIn, TIn, TIn>, EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TOut>> getOptions)
                where TInEf : class
                    => new EfCoreSaveStreamNode<TInEf, Correlated<TIn>, Correlated<TOut>>(name, getOptions(new EfCoreSaveCorrelatedArgsBuilder<TIn, TIn, TIn>(stream, i => i, (i, j) => i)).Args).Output;

        public static IStream<TIn> EfCoreSave<TIn>(this IStream<TIn> stream, string name) where TIn : class
            => new EfCoreSaveStreamNode<TIn, TIn, TIn>(name, new EfCoreSaveArgsBuilder<TIn, TIn, TIn>(stream, i => i, (i, j) => i).Args).Output;

        public static IStream<Correlated<TIn>> EfCoreSave<TIn>(this IStream<Correlated<TIn>> stream, string name) where TIn : class
            => new EfCoreSaveStreamNode<TIn, Correlated<TIn>, Correlated<TIn>>(name, new EfCoreSaveCorrelatedArgsBuilder<TIn, TIn, TIn>(stream, i => i, (i, j) => i).Args).Output;
        // copy/paste to explicitly solve ambiguity if any
        public static IStream<Correlated<TIn>> EfCoreSaveCorrelated<TIn>(this IStream<Correlated<TIn>> stream, string name) where TIn : class
            => new EfCoreSaveStreamNode<TIn, Correlated<TIn>, Correlated<TIn>>(name, new EfCoreSaveCorrelatedArgsBuilder<TIn, TIn, TIn>(stream, i => i, (i, j) => i).Args).Output;
    }
}