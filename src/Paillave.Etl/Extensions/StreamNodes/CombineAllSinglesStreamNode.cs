using System;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class CombineAllSinglesStreamNode<TIn1, TIn2, TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, (ISingleStream<TIn1>, ISingleStream<TIn2>, Func<TIn1, TIn2, TOut> map)>
    {
        public CombineAllSinglesStreamNode(string name, (ISingleStream<TIn1>, ISingleStream<TIn2>, Func<TIn1, TIn2, TOut> map) args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<TOut> CreateOutputStream((ISingleStream<TIn1>, ISingleStream<TIn2>, Func<TIn1, TIn2, TOut> map) args)
            => base.CreateSingleStream(args.Item1.Observable.CombineWithLatest(args.Item2.Observable, (i, j) => args.map(i, j)));
    }
    public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, (ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, Func<TIn1, TIn2, TIn3, TOut> map)>
    {
        public CombineAllSinglesStreamNode(string name, (ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, Func<TIn1, TIn2, TIn3, TOut> map) args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<TOut> CreateOutputStream((ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, Func<TIn1, TIn2, TIn3, TOut> map) args)
            => base.CreateSingleStream(args.Item1.Observable
                .CombineWithLatest(args.Item2.Observable, (i, j) => (i, j))
                    .CombineWithLatest(args.Item3.Observable, (i, j) => args.map(i.Item1, i.Item2, j)));
    }
    public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, (ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, Func<TIn1, TIn2, TIn3, TIn4, TOut> map)>
    {
        public CombineAllSinglesStreamNode(string name, (ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, Func<TIn1, TIn2, TIn3, TIn4, TOut> map) args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<TOut> CreateOutputStream((ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, Func<TIn1, TIn2, TIn3, TIn4, TOut> map) args)
            => base.CreateSingleStream(args.Item1.Observable
                .CombineWithLatest(args.Item2.Observable, (i, j) => (i, j))
                    .CombineWithLatest(args.Item3.Observable, (i, j) => (i.Item1, i.Item2, j))
                        .CombineWithLatest(args.Item4.Observable, (i, j) => args.map(i.Item1, i.Item2, i.Item3, j)));
    }
    public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, (ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> map)>
    {
        public CombineAllSinglesStreamNode(string name, (ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> map) args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<TOut> CreateOutputStream((ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> map) args)
            => base.CreateSingleStream(args.Item1.Observable
                .CombineWithLatest(args.Item2.Observable, (i, j) => (i, j))
                    .CombineWithLatest(args.Item3.Observable, (i, j) => (i.Item1, i.Item2, j))
                        .CombineWithLatest(args.Item4.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, j))
                            .CombineWithLatest(args.Item5.Observable, (i, j) => args.map(i.Item1, i.Item2, i.Item3, i.Item4, j)));
    }
    public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, (ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> map)>
    {
        public CombineAllSinglesStreamNode(string name, (ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> map) args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<TOut> CreateOutputStream((ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> map) args)
            => base.CreateSingleStream(args.Item1.Observable
                .CombineWithLatest(args.Item2.Observable, (i, j) => (i, j))
                    .CombineWithLatest(args.Item3.Observable, (i, j) => (i.Item1, i.Item2, j))
                        .CombineWithLatest(args.Item4.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, j))
                            .CombineWithLatest(args.Item5.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, i.Item4, j))
                                .CombineWithLatest(args.Item6.Observable, (i, j) => args.map(i.Item1, i.Item2, i.Item3, i.Item4, i.Item5, j)));
    }
    public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, (ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>, ISingleStream<TIn7>, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> map)>
    {
        public CombineAllSinglesStreamNode(string name, (ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>, ISingleStream<TIn7>, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> map) args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<TOut> CreateOutputStream((ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>, ISingleStream<TIn7>, Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> map) args)
            => base.CreateSingleStream(args.Item1.Observable
                .CombineWithLatest(args.Item2.Observable, (i, j) => (i, j))
                    .CombineWithLatest(args.Item3.Observable, (i, j) => (i.Item1, i.Item2, j))
                        .CombineWithLatest(args.Item4.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, j))
                            .CombineWithLatest(args.Item5.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, i.Item4, j))
                                .CombineWithLatest(args.Item6.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, i.Item4, i.Item5, j))
                                    .CombineWithLatest(args.Item7.Observable, (i, j) => args.map(i.Item1, i.Item2, i.Item3, i.Item4, i.Item5, i.Item6, j)));
    }
}
