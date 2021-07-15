using Paillave.Etl.Reactive.Operators;
using System;

namespace Paillave.Etl.Core
{
    public static partial class SelectSectionEx
    {
        public static IStream<TIn> SelectSection<TIn>(this IStream<TIn> stream, string name, KeepingState initialState, Func<TIn, SwitchBehavior> switcher)
        {
            return new SelectSectionStreamNode<TIn>(name, new SelectSectionArgs<TIn>
            {
                Stream = stream,
                InitialState = initialState,
                SwitchToKeep = switcher
            }).Output;
        }
        public static IStream<TIn> SelectSection<TIn>(this IStream<TIn> stream, string name, Func<TIn, SwitchBehavior> switcher)
        {
            return new SelectSectionStreamNode<TIn>(name, new SelectSectionArgs<TIn>
            {
                Stream = stream,
                SwitchToKeep = switcher
            }).Output;
        }
        public static IStream<TIn> SelectSection<TIn>(this IStream<TIn> stream, string name, KeepingState initialState, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore)
        {
            return new SelectSectionStreamNode<TIn>(name, new SelectSectionArgs<TIn>
            {
                Stream = stream,
                InitialState = initialState,
                SwitchToKeep = switchToKeep,
                SwitchToIgnore = switchToIgnore
            }).Output;
        }
        public static IStream<TIn> SelectSection<TIn>(this IStream<TIn> stream, string name, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore)
        {
            return new SelectSectionStreamNode<TIn>(name, new SelectSectionArgs<TIn>
            {
                Stream = stream,
                SwitchToKeep = switchToKeep,
                SwitchToIgnore = switchToIgnore
            }).Output;
        }













        public static IStream<TOut> SelectSection<TIn, TOut>(this IStream<TIn> stream, string name, KeepingState initialState, Func<TIn, SwitchBehavior> switcher, Func<TIn, int, TOut> resultSelector)
        {
            return new SelectSectionStreamNode<TIn, TOut>(name, new SelectSectionArgs<TIn, TOut>
            {
                Stream = stream,
                InitialState = initialState,
                SwitchToKeep = switcher,
                ResultSelector = resultSelector
            }).Output;
        }
        public static IStream<TOut> SelectSection<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, SwitchBehavior> switcher, Func<TIn, int, TOut> resultSelector)
        {
            return new SelectSectionStreamNode<TIn, TOut>(name, new SelectSectionArgs<TIn, TOut>
            {
                Stream = stream,
                SwitchToKeep = switcher,
                ResultSelector = resultSelector
            }).Output;
        }
        public static IStream<TOut> SelectSection<TIn, TOut>(this IStream<TIn> stream, string name, KeepingState initialState, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore, Func<TIn, int, TOut> resultSelector)
        {
            return new SelectSectionStreamNode<TIn, TOut>(name, new SelectSectionArgs<TIn, TOut>
            {
                Stream = stream,
                InitialState = initialState,
                SwitchToKeep = switchToKeep,
                SwitchToIgnore = switchToIgnore,
                ResultSelector = resultSelector
            }).Output;
        }
        public static IStream<TOut> SelectSection<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore, Func<TIn, int, TOut> resultSelector)
        {
            return new SelectSectionStreamNode<TIn, TOut>(name, new SelectSectionArgs<TIn, TOut>
            {
                Stream = stream,
                SwitchToKeep = switchToKeep,
                SwitchToIgnore = switchToIgnore,
                ResultSelector = resultSelector
            }).Output;
        }
    }
}
