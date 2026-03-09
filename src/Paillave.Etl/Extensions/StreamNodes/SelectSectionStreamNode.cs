using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;

namespace Paillave.Etl.Core;

public class SelectSectionArgs<TIn>
{
    public IStream<TIn> Stream { get; set; }
    public KeepingState? InitialState { get; set; } = null;
    public Func<TIn, SwitchBehavior> SwitchToKeep { get; set; } = null;
    public Func<TIn, SwitchBehavior> SwitchToIgnore { get; set; } = null;
}
public class SelectSectionArgs<TIn, TOut>
{
    public IStream<TIn> Stream { get; set; }
    public KeepingState? InitialState { get; set; } = null;
    public Func<TIn, SwitchBehavior> SwitchToKeep { get; set; } = null;
    public Func<TIn, SwitchBehavior> SwitchToIgnore { get; set; } = null;
    public Func<TIn, int, TOut> ResultSelector { get; set; }
}
public class SelectSectionStreamNode<TIn>(string name, SelectSectionArgs<TIn> args) : StreamNodeBase<TIn, IStream<TIn>, SelectSectionArgs<TIn>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override IStream<TIn> CreateOutputStream(SelectSectionArgs<TIn> args)
    {
        IPushObservable<TIn> obs;
        if (args.InitialState == null)
        {
            if (args.SwitchToIgnore == null)
                obs = args.Stream.Observable.FilterSection(args.SwitchToKeep);
            else
                obs = args.Stream.Observable.FilterSection(args.SwitchToKeep, args.SwitchToIgnore);
        }
        else
        {
            if (args.SwitchToIgnore == null)
                obs = args.Stream.Observable.FilterSection(args.InitialState.Value, args.SwitchToKeep);
            else
                obs = args.Stream.Observable.FilterSection(args.InitialState.Value, args.SwitchToKeep, args.SwitchToIgnore);
        }
        return base.CreateUnsortedStream(obs);
    }
}


public class SelectSectionStreamNode<TIn, TOut>(string name, SelectSectionArgs<TIn, TOut> args) : StreamNodeBase<TOut, IStream<TOut>, SelectSectionArgs<TIn, TOut>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override IStream<TOut> CreateOutputStream(SelectSectionArgs<TIn, TOut> args)
    {
        IPushObservable<TOut> obs;
        if (args.InitialState == null)
        {
            if (args.SwitchToIgnore == null)
                obs = args.Stream.Observable.FilterSection(args.SwitchToKeep, args.ResultSelector);
            else
                obs = args.Stream.Observable.FilterSection(args.SwitchToKeep, args.SwitchToIgnore, args.ResultSelector);
        }
        else
        {
            if (args.SwitchToIgnore == null)
                obs = args.Stream.Observable.FilterSection(args.InitialState.Value, args.SwitchToKeep, args.ResultSelector);
            else
                obs = args.Stream.Observable.FilterSection(args.InitialState.Value, args.SwitchToKeep, args.SwitchToIgnore, args.ResultSelector);
        }
        return base.CreateUnsortedStream(obs);
    }
}
