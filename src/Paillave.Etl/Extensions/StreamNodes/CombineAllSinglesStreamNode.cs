using System;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core;

public class CombineAllSinglesArgs<TIn1, TIn2, TOut>
{
    public ISingleStream<TIn1> Item1 { get; set; }
    public ISingleStream<TIn2> Item2 { get; set; }
    public Func<TIn1, TIn2, TOut> Map { get; set; }
}
public class CombineAllSinglesStreamNode<TIn1, TIn2, TOut>(string name, CombineAllSinglesArgs<TIn1, TIn2, TOut> args) : StreamNodeBase<TOut, ISingleStream<TOut>, CombineAllSinglesArgs<TIn1, TIn2, TOut>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override ISingleStream<TOut> CreateOutputStream(CombineAllSinglesArgs<TIn1, TIn2, TOut> args)
        => base.CreateSingleStream(args.Item1.Observable.CombineWithLatest(args.Item2.Observable, (i, j) => args.Map(i, j)));
}
public class CombineAllSinglesArgs<TIn1, TIn2, TIn3, TOut>
{
    public ISingleStream<TIn1> Item1 { get; set; }
    public ISingleStream<TIn2> Item2 { get; set; }
    public ISingleStream<TIn3> Item3 { get; set; }
    public Func<TIn1, TIn2, TIn3, TOut> Map { get; set; }
}
public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TOut>(string name, CombineAllSinglesArgs<TIn1, TIn2, TIn3, TOut> args) : StreamNodeBase<TOut, ISingleStream<TOut>, CombineAllSinglesArgs<TIn1, TIn2, TIn3, TOut>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override ISingleStream<TOut> CreateOutputStream(CombineAllSinglesArgs<TIn1, TIn2, TIn3, TOut> args)
        => base.CreateSingleStream(args.Item1.Observable
            .CombineWithLatest(args.Item2.Observable, (i, j) => (i, j))
                .CombineWithLatest(args.Item3.Observable, (i, j) => args.Map(i.Item1, i.Item2, j)));
}
public class CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TOut>
{
    public ISingleStream<TIn1> Item1 { get; set; }
    public ISingleStream<TIn2> Item2 { get; set; }
    public ISingleStream<TIn3> Item3 { get; set; }
    public ISingleStream<TIn4> Item4 { get; set; }
    public Func<TIn1, TIn2, TIn3, TIn4, TOut> Map { get; set; }
}
public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TOut>(string name, CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TOut> args) : StreamNodeBase<TOut, ISingleStream<TOut>, CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TOut>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override ISingleStream<TOut> CreateOutputStream(CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TOut> args)
        => base.CreateSingleStream(args.Item1.Observable
            .CombineWithLatest(args.Item2.Observable, (i, j) => (i, j))
                .CombineWithLatest(args.Item3.Observable, (i, j) => (i.Item1, i.Item2, j))
                    .CombineWithLatest(args.Item4.Observable, (i, j) => args.Map(i.Item1, i.Item2, i.Item3, j)));
}
public class CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>
{
    public ISingleStream<TIn1> Item1 { get; set; }
    public ISingleStream<TIn2> Item2 { get; set; }
    public ISingleStream<TIn3> Item3 { get; set; }
    public ISingleStream<TIn4> Item4 { get; set; }
    public ISingleStream<TIn5> Item5 { get; set; }
    public Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> Map { get; set; }
}
public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(string name, CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> args) : StreamNodeBase<TOut, ISingleStream<TOut>, CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override ISingleStream<TOut> CreateOutputStream(CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> args)
        => base.CreateSingleStream(args.Item1.Observable
            .CombineWithLatest(args.Item2.Observable, (i, j) => (i, j))
                .CombineWithLatest(args.Item3.Observable, (i, j) => (i.Item1, i.Item2, j))
                    .CombineWithLatest(args.Item4.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, j))
                        .CombineWithLatest(args.Item5.Observable, (i, j) => args.Map(i.Item1, i.Item2, i.Item3, i.Item4, j)));
}
public class CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>
{
    public ISingleStream<TIn1> Item1 { get; set; }
    public ISingleStream<TIn2> Item2 { get; set; }
    public ISingleStream<TIn3> Item3 { get; set; }
    public ISingleStream<TIn4> Item4 { get; set; }
    public ISingleStream<TIn5> Item5 { get; set; }
    public ISingleStream<TIn6> Item6 { get; set; }
    public Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> Map { get; set; }
}
public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>(string name, CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> args) : StreamNodeBase<TOut, ISingleStream<TOut>, CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override ISingleStream<TOut> CreateOutputStream(CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> args)
        => base.CreateSingleStream(args.Item1.Observable
            .CombineWithLatest(args.Item2.Observable, (i, j) => (i, j))
                .CombineWithLatest(args.Item3.Observable, (i, j) => (i.Item1, i.Item2, j))
                    .CombineWithLatest(args.Item4.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, j))
                        .CombineWithLatest(args.Item5.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, i.Item4, j))
                            .CombineWithLatest(args.Item6.Observable, (i, j) => args.Map(i.Item1, i.Item2, i.Item3, i.Item4, i.Item5, j)));
}
public class CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>
{
    public ISingleStream<TIn1> Item1 { get; set; }
    public ISingleStream<TIn2> Item2 { get; set; }
    public ISingleStream<TIn3> Item3 { get; set; }
    public ISingleStream<TIn4> Item4 { get; set; }
    public ISingleStream<TIn5> Item5 { get; set; }
    public ISingleStream<TIn6> Item6 { get; set; }
    public ISingleStream<TIn7> Item7 { get; set; }
    public Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> Map { get; set; }
}
public class CombineAllSinglesStreamNode<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>(string name, CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> args) : StreamNodeBase<TOut, ISingleStream<TOut>, CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override ISingleStream<TOut> CreateOutputStream(CombineAllSinglesArgs<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> args)
        => base.CreateSingleStream(args.Item1.Observable
            .CombineWithLatest(args.Item2.Observable, (i, j) => (i, j))
                .CombineWithLatest(args.Item3.Observable, (i, j) => (i.Item1, i.Item2, j))
                    .CombineWithLatest(args.Item4.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, j))
                        .CombineWithLatest(args.Item5.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, i.Item4, j))
                            .CombineWithLatest(args.Item6.Observable, (i, j) => (i.Item1, i.Item2, i.Item3, i.Item4, i.Item5, j))
                                .CombineWithLatest(args.Item7.Observable, (i, j) => args.Map(i.Item1, i.Item2, i.Item3, i.Item4, i.Item5, i.Item6, j)));
}
