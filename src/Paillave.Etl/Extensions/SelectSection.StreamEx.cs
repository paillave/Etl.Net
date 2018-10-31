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

namespace Paillave.Etl.Extensions
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
