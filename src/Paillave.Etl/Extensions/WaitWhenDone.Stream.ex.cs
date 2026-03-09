using System;

namespace Paillave.Etl.Core;

public static partial class WaitWhenDoneEx
{
    public static ISortedStream<TIn, TKey> WaitWhenDone<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, params IStream<object>[] streamToWait)
    {
        return new WaitWhenDoneStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new WaitWhenDoneArgs<TIn, ISortedStream<TIn, TKey>>
        {
            Input = stream,
            Input1ToWait = streamToWait.Length >= 1 ? streamToWait[0] : null,
            Input2ToWait = streamToWait.Length >= 2 ? streamToWait[1] : null,
            Input3ToWait = streamToWait.Length >= 3 ? streamToWait[2] : null,
            Input4ToWait = streamToWait.Length >= 4 ? streamToWait[3] : null,
            Input5ToWait = streamToWait.Length >= 5 ? streamToWait[4] : null,
            Input6ToWait = streamToWait.Length >= 6 ? streamToWait[5] : null,
            Input7ToWait = streamToWait.Length >= 7 ? streamToWait[6] : null,
            Input8ToWait = streamToWait.Length >= 8 ? streamToWait[7] : null,
            Input9ToWait = streamToWait.Length >= 9 ? streamToWait[8] : null,
            Input10ToWait = streamToWait.Length >= 10 ? streamToWait[9] : null,
        }).Output;
    }
    public static IKeyedStream<TIn, TKey> WaitWhenDone<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, params IStream<object>[] streamToWait)
    {
        return new WaitWhenDoneStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new WaitWhenDoneArgs<TIn, IKeyedStream<TIn, TKey>>
        {
            Input = stream,
            Input1ToWait = streamToWait.Length >= 1 ? streamToWait[0] : null,
            Input2ToWait = streamToWait.Length >= 2 ? streamToWait[1] : null,
            Input3ToWait = streamToWait.Length >= 3 ? streamToWait[2] : null,
            Input4ToWait = streamToWait.Length >= 4 ? streamToWait[3] : null,
            Input5ToWait = streamToWait.Length >= 5 ? streamToWait[4] : null,
            Input6ToWait = streamToWait.Length >= 6 ? streamToWait[5] : null,
            Input7ToWait = streamToWait.Length >= 7 ? streamToWait[6] : null,
            Input8ToWait = streamToWait.Length >= 8 ? streamToWait[7] : null,
            Input9ToWait = streamToWait.Length >= 9 ? streamToWait[8] : null,
            Input10ToWait = streamToWait.Length >= 10 ? streamToWait[9] : null,
        }).Output;
    }
    public static IStream<TIn> WaitWhenDone<TIn>(this IStream<TIn> stream, string name, params IStream<object>[] streamToWait)
    {
        return new WaitWhenDoneStreamNode<TIn, IStream<TIn>>(name, new WaitWhenDoneArgs<TIn, IStream<TIn>>
        {
            Input = stream,
            Input1ToWait = streamToWait.Length >= 1 ? streamToWait[0] : null,
            Input2ToWait = streamToWait.Length >= 2 ? streamToWait[1] : null,
            Input3ToWait = streamToWait.Length >= 3 ? streamToWait[2] : null,
            Input4ToWait = streamToWait.Length >= 4 ? streamToWait[3] : null,
            Input5ToWait = streamToWait.Length >= 5 ? streamToWait[4] : null,
            Input6ToWait = streamToWait.Length >= 6 ? streamToWait[5] : null,
            Input7ToWait = streamToWait.Length >= 7 ? streamToWait[6] : null,
            Input8ToWait = streamToWait.Length >= 8 ? streamToWait[7] : null,
            Input9ToWait = streamToWait.Length >= 9 ? streamToWait[8] : null,
            Input10ToWait = streamToWait.Length >= 10 ? streamToWait[9] : null,
        }).Output;
    }
    public static ISingleStream<TIn> WaitWhenDone<TIn>(this ISingleStream<TIn> stream, string name, params IStream<object>[] streamToWait)
    {
        return new WaitWhenDoneStreamNode<TIn, ISingleStream<TIn>>(name, new WaitWhenDoneArgs<TIn, ISingleStream<TIn>>
        {
            Input = stream,
            Input1ToWait = streamToWait.Length >= 1 ? streamToWait[0] : null,
            Input2ToWait = streamToWait.Length >= 2 ? streamToWait[1] : null,
            Input3ToWait = streamToWait.Length >= 3 ? streamToWait[2] : null,
            Input4ToWait = streamToWait.Length >= 4 ? streamToWait[3] : null,
            Input5ToWait = streamToWait.Length >= 5 ? streamToWait[4] : null,
            Input6ToWait = streamToWait.Length >= 6 ? streamToWait[5] : null,
            Input7ToWait = streamToWait.Length >= 7 ? streamToWait[6] : null,
            Input8ToWait = streamToWait.Length >= 8 ? streamToWait[7] : null,
            Input9ToWait = streamToWait.Length >= 9 ? streamToWait[8] : null,
            Input10ToWait = streamToWait.Length >= 10 ? streamToWait[9] : null,
        }).Output;
    }
}
