using System;

namespace Paillave.Etl.Core
{
    public static partial class FixEx
    {
        public static IStream<T> Fix<T>(this IStream<T> stream, string name, Func<Fixer<T>, Fixer<T>> fixer) =>
            new FixStreamNode<T>(name, new FixArgs<T>
            {
                Fixer = fixer(new Fixer<T>()),
                Stream = stream
            }).Output;
        public static IStream<Correlated<T>> Fix<T>(this IStream<Correlated<T>> stream, string name, Func<Fixer<T>, Fixer<T>> fixer) =>
            new FixCorrelatedStreamNode<T>(name, new FixCorrelatedArgs<T>
            {
                Fixer = fixer(new Fixer<T>()),
                Stream = stream
            }).Output;
        public static ISingleStream<T> FixNull<T>(this ISingleStream<T> stream, string name, Func<Fixer<T>, Fixer<T>> fixer) =>
            new FixSingleStreamNode<T>(name, new FixSingleArgs<T>
            {
                Fixer = fixer(new Fixer<T>()),
                Stream = stream
            }).Output;
        public static ISingleStream<Correlated<T>> FixNull<T>(this ISingleStream<Correlated<T>> stream, string name, Func<Fixer<T>, Fixer<T>> fixer) =>
            new FixCorrelatedSingleStreamNode<T>(name, new FixCorrelatedSingleArgs<T>
            {
                Fixer = fixer(new Fixer<T>()),
                Stream = stream
            }).Output;
    }
}
