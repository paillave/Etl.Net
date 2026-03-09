using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paillave.Etl.Core;

public class LookupArgs<TInLeft, TInRight, TOut, TKey>
{
    public IStream<TInLeft> LeftInputStream { get; set; }
    public IStream<TInRight> RightInputStream { get; set; }
    public Func<TInLeft, TKey> GetLeftStreamKey { get; set; }
    public Func<TInRight, TKey> GetRightStreamKey { get; set; }
    public Func<TInLeft, TInRight, TOut> ResultSelector { get; set; }
}
public class LookupStreamNode<TInLeft, TInRight, TOut, TKey>(string name, LookupArgs<TInLeft, TInRight, TOut, TKey> args) : StreamNodeBase<TOut, IStream<TOut>, LookupArgs<TInLeft, TInRight, TOut, TKey>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

    protected override IStream<TOut> CreateOutputStream(LookupArgs<TInLeft, TInRight, TOut, TKey> args)
    {
        var rightDicoS = args.RightInputStream.Observable.ToList().Map(l => l.Select(i => new { Key = args.GetRightStreamKey(i), Value = i }).Where(i => i.Key != null).ToDictionary(i => i.Key, i => i.Value));
        var matchingS = args.LeftInputStream.Observable.CombineWithLatest(rightDicoS, (l, rl) => new { Left = l, Right = this.HandleMatching(l, rl, args.GetLeftStreamKey) }, true);
        return base.CreateUnsortedStream(matchingS.Map(i => args.ResultSelector(i.Left, i.Right)));
    }
    private TInRight HandleMatching(TInLeft l, Dictionary<TKey, TInRight> rl, Func<TInLeft, TKey> getLeftStreamKey)
    {
        var key = getLeftStreamKey(l);
        if (key == null) return default;
        rl.TryGetValue(key, out TInRight r);
        return r;
    }
}
