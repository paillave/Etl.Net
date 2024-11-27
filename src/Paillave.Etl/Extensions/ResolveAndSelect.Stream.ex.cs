using System;

namespace Paillave.Etl.Core
{
    public static class SelectWithContextBagEx
    {
        public static IStream<TOut> ResolveAndSelect<TIn, TService, TOut>(this IStream<TIn> stream, string name, Func<Resolver<TIn>, Selector<TIn, TService, TOut>> selection, bool withNoDispose = false) where TService : class =>
            new ResolveAndSelectStreamNode<TIn, TService, TOut>(name, new ResolveAndSelectArgs<TIn, TService, TOut>
            {
                Stream = stream,
                Selection = selection,
                WithNoDispose = withNoDispose
            }).Output;
        public static ISingleStream<TOut> ResolveAndSelect<TIn, TService, TOut>(this ISingleStream<TIn> stream, string name, Func<Resolver<TIn>, Selector<TIn, TService, TOut>> selection, bool withNoDispose = false) where TService : class =>
            new ResolveAndSelectSingleStreamNode<TIn, TService, TOut>(name, new ResolveAndSelectSingleArgs<TIn, TService, TOut>
            {
                Stream = stream,
                Selection = selection,
                WithNoDispose = withNoDispose
            }).Output;
        public static IStream<Correlated<TOut>> ResolveAndSelect<TIn, TService, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<Resolver<TIn>, Selector<TIn, TService, TOut>> selection, bool withNoDispose = false) where TService : class =>
            new ResolveAndSelectCorrelatedStreamNode<TIn, TService, TOut>(name, new ResolveAndSelectCorrelatedArgs<TIn, TService, TOut>
            {
                Stream = stream,
                Selection = selection,
                WithNoDispose = withNoDispose
            }).Output;
        public static ISingleStream<Correlated<TOut>> ResolveAndSelect<TIn, TService, TOut>(this ISingleStream<Correlated<TIn>> stream, string name, Func<Resolver<TIn>, Selector<TIn, TService, TOut>> selection, bool withNoDispose = false) where TService : class =>
            new ResolveAndSelectCorrelatedSingleStreamNode<TIn, TService, TOut>(name, new ResolveAndSelectCorrelatedSingleArgs<TIn, TService, TOut>
            {
                Stream = stream,
                Selection = selection,
                WithNoDispose = withNoDispose
            }).Output;
    }
}