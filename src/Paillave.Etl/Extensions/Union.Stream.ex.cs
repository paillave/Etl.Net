using System;

namespace Paillave.Etl.Core
{
    public static partial class UnionEx
    {
        public static IStream<I> Union<I>(this IStream<I> stream, string name, IStream<I> inputStream2)
        {
            return new UnionStreamNode<I>(name, new UnionArgs<I>
            {
                Stream1 = stream,
                Stream2 = inputStream2
            }).Output;
        }

        public static IStream<TOut> Union<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TOut> resultSelectorLeft, Func<TIn2, TOut> resultSelectorRight)
        {
            return new UnionStreamNode<TIn1, TIn2, TOut>(name, new UnionArgs<TIn1, TIn2, TOut>
            {
                Stream1 = stream,
                Stream2 = inputStream2,
                ResultSelectorLeft = resultSelectorLeft,
                ResultSelectorRight = resultSelectorRight
            }).Output;
        }
        public static IStream<Correlated<TOut>> Union<TIn1, TIn2, TOut>(this IStream<Correlated<TIn1>> stream, string name, IStream<Correlated<TIn2>> inputStream2, Func<TIn1, TOut> resultSelectorLeft, Func<TIn2, TOut> resultSelectorRight)
        {
            return new UnionStreamNode<Correlated<TIn1>, Correlated<TIn2>, Correlated<TOut>>(name, new UnionArgs<Correlated<TIn1>, Correlated<TIn2>, Correlated<TOut>>
            {
                Stream1 = stream,
                Stream2 = inputStream2,
                ResultSelectorLeft = i => new Correlated<TOut> { Row = resultSelectorLeft(i.Row), CorrelationKeys = i.CorrelationKeys },
                ResultSelectorRight = i => new Correlated<TOut> { Row = resultSelectorRight(i.Row), CorrelationKeys = i.CorrelationKeys }
            }).Output;
        }

        public static IStream<TOut> Union<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelector)
        {
            return new UnionStreamNode<TIn1, TIn2, TOut>(name, new UnionArgs<TIn1, TIn2, TOut>
            {
                Stream1 = stream,
                Stream2 = inputStream2,
                FullResultSelectorLeft = resultSelector
            }).Output;
        }
        public static IStream<Correlated<TOut>> Union<TIn1, TIn2, TOut>(this IStream<Correlated<TIn1>> stream, string name, IStream<Correlated<TIn2>> inputStream2, Func<TIn1, TIn2, TOut> resultSelector)
        {
            return new UnionStreamNode<Correlated<TIn1>, Correlated<TIn2>, Correlated<TOut>>(name, new UnionArgs<Correlated<TIn1>, Correlated<TIn2>, Correlated<TOut>>
            {
                Stream1 = stream,
                Stream2 = inputStream2,
                FullResultSelectorLeft = (l, r) => new Correlated<TOut>
                {
                    Row = resultSelector(l == null ? default : l.Row, r == null ? default : r.Row),
                    CorrelationKeys = l?.CorrelationKeys ?? r?.CorrelationKeys
                }
            }).Output;
        }

        public static IStream<TOut> Union<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelectorLeft, Func<TIn1, TIn2, TOut> resultSelectorRight)
        {
            return new UnionStreamNode<TIn1, TIn2, TOut>(name, new UnionArgs<TIn1, TIn2, TOut>
            {
                Stream1 = stream,
                Stream2 = inputStream2,
                FullResultSelectorLeft = resultSelectorLeft,
                FullResultSelectorRight = resultSelectorRight,
            }).Output;
        }
        public static IStream<Correlated<TOut>> Union<TIn1, TIn2, TOut>(this IStream<Correlated<TIn1>> stream, string name, IStream<Correlated<TIn2>> inputStream2, Func<TIn1, TIn2, TOut> resultSelectorLeft, Func<TIn1, TIn2, TOut> resultSelectorRight)
        {
            return new UnionStreamNode<Correlated<TIn1>, Correlated<TIn2>, Correlated<TOut>>(name, new UnionArgs<Correlated<TIn1>, Correlated<TIn2>, Correlated<TOut>>
            {
                Stream1 = stream,
                Stream2 = inputStream2,
                FullResultSelectorLeft = (l, r) => new Correlated<TOut>
                {
                    Row = resultSelectorLeft(l == null ? default : l.Row, r == null ? default : r.Row),
                    CorrelationKeys = l?.CorrelationKeys ?? r?.CorrelationKeys
                },
                FullResultSelectorRight = (l, r) => new Correlated<TOut>
                {
                    Row = resultSelectorRight(l == null ? default : l.Row, r == null ? default : r.Row),
                    CorrelationKeys = l?.CorrelationKeys ?? r?.CorrelationKeys
                }
            }).Output;
        }

        public static IStream<TOut> Union<TIn, TOut>(this IStream<TIn> stream, string name, params Func<ISingleStream<TIn>, IStream<TOut>>[] subProcesses)
        {
            return new ToSubProcessesStreamNode<TIn, TOut>(name, new ToSubProcessesArgs<TIn, TOut>
            {
                Stream = stream,
                NoParallelisation = false,
                SubProcesses = subProcesses
            }).Output;
        }
    }
}