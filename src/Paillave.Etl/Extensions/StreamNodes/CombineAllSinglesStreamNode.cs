using System;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class CombineAllSinglesStreamNode<TIn1, TIn2> : StreamNodeBase<Tuple<TIn1, TIn2>, ISingleStream<Tuple<TIn1, TIn2>>, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>>>
    {
        public CombineAllSinglesStreamNode(string name, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<Tuple<TIn1, TIn2>> CreateOutputStream(Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>> args)
            => base.CreateSingleStream(args.Item1.Observable.CombineWithLatest(args.Item2.Observable, (i, j) => new Tuple<TIn1, TIn2>(i, j)));
    }
    public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3> : StreamNodeBase<Tuple<TIn1, TIn2, TIn3>, ISingleStream<Tuple<TIn1, TIn2, TIn3>>, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>>>
    {
        public CombineAllSinglesStreamNode(string name, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<Tuple<TIn1, TIn2, TIn3>> CreateOutputStream(Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>> args)
            => base.CreateSingleStream(args.Item1.Observable
                .CombineWithLatest(args.Item2.Observable, (i, j) => new Tuple<TIn1, TIn2>(i, j))
                    .CombineWithLatest(args.Item3.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3>(i.Item1, i.Item2, j)));
    }
    public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4> : StreamNodeBase<Tuple<TIn1, TIn2, TIn3, TIn4>, ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4>>, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>>>
    {
        public CombineAllSinglesStreamNode(string name, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4>> CreateOutputStream(Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>> args)
            => base.CreateSingleStream(args.Item1.Observable
                .CombineWithLatest(args.Item2.Observable, (i, j) => new Tuple<TIn1, TIn2>(i, j))
                    .CombineWithLatest(args.Item3.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3>(i.Item1, i.Item2, j))
                        .CombineWithLatest(args.Item4.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3, TIn4>(i.Item1, i.Item2, i.Item3, j)));
    }
    public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5> : StreamNodeBase<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5>, ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5>>, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>>>
    {
        public CombineAllSinglesStreamNode(string name, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5>> CreateOutputStream(Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>> args)
            => base.CreateSingleStream(args.Item1.Observable
                .CombineWithLatest(args.Item2.Observable, (i, j) => new Tuple<TIn1, TIn2>(i, j))
                    .CombineWithLatest(args.Item3.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3>(i.Item1, i.Item2, j))
                        .CombineWithLatest(args.Item4.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3, TIn4>(i.Item1, i.Item2, i.Item3, j))
                            .CombineWithLatest(args.Item5.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3, TIn4, TIn5>(i.Item1, i.Item2, i.Item3, i.Item4, j)));
    }
    public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6> : StreamNodeBase<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>, ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>>, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>>>
    {
        public CombineAllSinglesStreamNode(string name, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>> CreateOutputStream(Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>> args)
            => base.CreateSingleStream(args.Item1.Observable
                .CombineWithLatest(args.Item2.Observable, (i, j) => new Tuple<TIn1, TIn2>(i, j))
                    .CombineWithLatest(args.Item3.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3>(i.Item1, i.Item2, j))
                        .CombineWithLatest(args.Item4.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3, TIn4>(i.Item1, i.Item2, i.Item3, j))
                            .CombineWithLatest(args.Item5.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3, TIn4, TIn5>(i.Item1, i.Item2, i.Item3, i.Item4, j))
                                .CombineWithLatest(args.Item6.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(i.Item1, i.Item2, i.Item3, i.Item4, i.Item5, j)));
    }
    public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7> : StreamNodeBase<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>, ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>>, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>, ISingleStream<TIn7>>>
    {
        public CombineAllSinglesStreamNode(string name, Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>, ISingleStream<TIn7>> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>> CreateOutputStream(Tuple<ISingleStream<TIn1>, ISingleStream<TIn2>, ISingleStream<TIn3>, ISingleStream<TIn4>, ISingleStream<TIn5>, ISingleStream<TIn6>, ISingleStream<TIn7>> args)
            => base.CreateSingleStream(args.Item1.Observable
                .CombineWithLatest(args.Item2.Observable, (i, j) => new Tuple<TIn1, TIn2>(i, j))
                    .CombineWithLatest(args.Item3.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3>(i.Item1, i.Item2, j))
                        .CombineWithLatest(args.Item4.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3, TIn4>(i.Item1, i.Item2, i.Item3, j))
                            .CombineWithLatest(args.Item5.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3, TIn4, TIn5>(i.Item1, i.Item2, i.Item3, i.Item4, j))
                                .CombineWithLatest(args.Item6.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(i.Item1, i.Item2, i.Item3, i.Item4, i.Item5, j))
                                    .CombineWithLatest(args.Item7.Observable, (i, j) => new Tuple<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>(i.Item1, i.Item2, i.Item3, i.Item4, i.Item5, i.Item6, j)));
    }
}
