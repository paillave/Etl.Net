using System;

namespace Paillave.Etl.Core
{
    public static partial class CombineAllSinglesEx
    {
        public static ISingleStream<TOut> Combine<TIn1, TIn2, TOut>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2, Func<TIn1, TIn2, TOut> map)
            => new CombineAllSinglesStreamNode<TIn1, TIn2, TOut>(name, new CombineAllSinglesArgs<TIn1, TIn2, TOut>
            {
                Item1 = stream1,
                Item2 = stream2,
                Map = map
            }).Output;
        public static ISingleStream<TOut> Combine<TIn1, TIn2, TIn3, TOut>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2, ISingleStream<TIn3> stream3, Func<TIn1, TIn2, TIn3, TOut> map)
            => new CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TOut>(name, new CombineAllSinglesArgs<TIn1, TIn2, TIn3, TOut>
            {
                Item1 = stream1,
                Item2 = stream2,
                Item3 = stream3,
                Map = map
            }).Output;
        public static ISingleStream<TOut> Combine<TIn1, TIn2, TIn3, TIn4, TOut>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2, ISingleStream<TIn3> stream3, ISingleStream<TIn4> stream4, Func<TIn1, TIn2, TIn3, TIn4, TOut> map)
            => new CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TOut>(name, new CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TOut>
            {
                Item1 = stream1,
                Item2 = stream2,
                Item3 = stream3,
                Item4 = stream4,
                Map = map
            }).Output;
        public static ISingleStream<TOut> Combine<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2, ISingleStream<TIn3> stream3, ISingleStream<TIn4> stream4, ISingleStream<TIn5> stream5, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> map)
            => new CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(name, new CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>
            {
                Item1 = stream1,
                Item2 = stream2,
                Item3 = stream3,
                Item4 = stream4,
                Item5 = stream5,
                Map = map
            }).Output;
        public static ISingleStream<TOut> Combine<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2, ISingleStream<TIn3> stream3, ISingleStream<TIn4> stream4, ISingleStream<TIn5> stream5, ISingleStream<TIn6> stream6, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> map)
            => new CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>(name, new CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>
            {
                Item1 = stream1,
                Item2 = stream2,
                Item3 = stream3,
                Item4 = stream4,
                Item5 = stream5,
                Item6 = stream6,
                Map = map
            }).Output;
        public static ISingleStream<TOut> Combine<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>(this ISingleStream<TIn1> stream1, string name, ISingleStream<TIn2> stream2, ISingleStream<TIn3> stream3, ISingleStream<TIn4> stream4, ISingleStream<TIn5> stream5, ISingleStream<TIn6> stream6, ISingleStream<TIn7> stream7, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> map)
            => new CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>(name, new CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>
            {
                Item1 = stream1,
                Item2 = stream2,
                Item3 = stream3,
                Item4 = stream4,
                Item5 = stream5,
                Item6 = stream6,
                Item7 = stream7,
                Map = map
            }).Output;
    }
}
