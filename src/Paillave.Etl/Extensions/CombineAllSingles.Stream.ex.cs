using System;

namespace Paillave.Etl.Core
{
    public static partial class CombineAllSinglesEx
    {
        public static ISingleStream<Tuple<TIn1, TIn2>> Combine<TIn1, TIn2>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2)
            => new CombineAllSinglesStreamNode<TIn1, TIn2>(name, new Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>>(stream1, stream2)).Output;
        public static ISingleStream<Tuple<TIn1, TIn2, TIn3>> Combine<TIn1, TIn2, TIn3>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2, ISingleStream<TIn3> stream3)
            => new CombineAllSinglesStreamNode<TIn1, TIn2, TIn3>(name, new Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>>(stream1, stream2, stream3)).Output;
        public static ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4>> Combine<TIn1, TIn2, TIn3, TIn4>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2, ISingleStream<TIn3> stream3, ISingleStream<TIn4> stream4)
            => new CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4>(name, new Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>>(stream1, stream2, stream3, stream4)).Output;
        public static ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5>> Combine<TIn1, TIn2, TIn3, TIn4, TIn5>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2, ISingleStream<TIn3> stream3, ISingleStream<TIn4> stream4, ISingleStream<TIn5> stream5)
            => new CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5>(name, new Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>>(stream1, stream2, stream3, stream4, stream5)).Output;
        public static ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>> Combine<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2, ISingleStream<TIn3> stream3, ISingleStream<TIn4> stream4, ISingleStream<TIn5> stream5, ISingleStream<TIn6> stream6)
            => new CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(name, new Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>>(stream1, stream2, stream3, stream4, stream5, stream6)).Output;
        public static ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>> Combine<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2, ISingleStream<TIn3> stream3, ISingleStream<TIn4> stream4, ISingleStream<TIn5> stream5, ISingleStream<TIn6> stream6, ISingleStream<TIn7> stream7)
            => new CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>(name, new Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>, ISingleStream<TIn7>>(stream1, stream2, stream3, stream4, stream5, stream6, stream7)).Output;
    }
}
