using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class SubstractArgs<TInLeft, TInRight, TKey>
    {
        public ISortedStream<TInLeft, TKey> LeftInputStream { get; set; }
        public ISortedStream<TInRight, TKey> RightInputStream { get; set; }
    }
    public class SubstractUnsortedArgs<TStream, TInLeft, TInRight, TKey> where TStream : IStream<TInLeft>
    {
        public TStream LeftInputStream { get; set; }
        public IStream<TInRight> RightInputStream { get; set; }
        public Func<TInLeft, TKey> GetLeftKey { get; set; }
        public Func<TInRight, TKey> GetRightKey { get; set; }
    }
    public class SubstractStreamNode<TInLeft, TInRight, TKey> : StreamNodeBase<TInLeft, IStream<TInLeft>, SubstractArgs<TInLeft, TInRight, TKey>>
    {
        public SubstractStreamNode(string name, SubstractArgs<TInLeft, TInRight, TKey> args) : base(name, args)
        {
        }
        protected override IStream<TInLeft> CreateOutputStream(SubstractArgs<TInLeft, TInRight, TKey> args)
        {
            return base.CreateSortedStream(args.LeftInputStream.Observable.Substract<TInLeft, TInRight, TKey>(args.RightInputStream.Observable, new SortDefinitionComparer<TInLeft, TInRight, TKey>(args.LeftInputStream.SortDefinition, args.RightInputStream.SortDefinition)), args.LeftInputStream.SortDefinition);
        }
    }
    public class SubstractUnsortedStreamNode<TStream, TInLeft, TInRight, TKey> : StreamNodeBase<TInLeft, TStream, SubstractUnsortedArgs<TStream, TInLeft, TInRight, TKey>> where TStream : IStream<TInLeft>
    {
        public SubstractUnsortedStreamNode(string name, SubstractUnsortedArgs<TStream, TInLeft, TInRight, TKey> args) : base(name, args)
        {
        }
        protected override TStream CreateOutputStream(SubstractUnsortedArgs<TStream, TInLeft, TInRight, TKey> args)
        {
            var keyToExcludeHashObservable = args.RightInputStream.Observable.ToList().Map(i => new HashSet<TKey>(i.Select(j => args.GetRightKey(j)).Distinct()));
            var outObservable = args.LeftInputStream.Observable
                .CombineWithLatest(
                    keyToExcludeHashObservable,
                    (l, r) => new
                    {
                        Value = l,
                        MustExclude = r.Contains(args.GetLeftKey(l))
                    })
                .Filter(i => !i.MustExclude)
                .Map(i => i.Value);
            return base.CreateMatchingStream(outObservable, args.LeftInputStream);
        }
    }
}
