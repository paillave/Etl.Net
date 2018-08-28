using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class LookupArgs<TInLeft, TInRight, TOut, TKey>
    {
        public IStream<TInLeft> LeftInputStream { get; set; }
        public IStream<TInRight> RightInputStream { get; set; }
        public Func<TInLeft, TKey> GetLeftStreamKey { get; set; }
        public Func<TInRight, TKey> GetRightStreamKey { get; set; }
        public Func<TInLeft, TInRight, TOut> ResultSelector { get; set; }
    }
    public class LookupStreamNode<TInLeft, TInRight, TOut, TKey> : StreamNodeBase<TOut, IStream<TOut>, LookupArgs<TInLeft, TInRight, TOut, TKey>>
    {
        public LookupStreamNode(string name, LookupArgs<TInLeft, TInRight, TOut, TKey> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(LookupArgs<TInLeft, TInRight, TOut, TKey> args)
        {
            var rightDicoS = args.RightInputStream.Observable.ToList().Map(l => l.ToDictionary(args.GetRightStreamKey));
            var matchingS = args.LeftInputStream.Observable.CombineWithLatest(rightDicoS, (l, rl) => new { Left = l, Right = this.HandleMatching(l, rl, args.GetLeftStreamKey) }, true);
            return base.CreateStream(matchingS.Map(i => args.ResultSelector(i.Left, i.Right)));
        }
        private TInRight HandleMatching(TInLeft l, Dictionary<TKey, TInRight> rl, Func<TInLeft, TKey> getLeftStreamKey)
        {
            rl.TryGetValue(getLeftStreamKey(l), out TInRight r);
            return r;
        }
    }
}
