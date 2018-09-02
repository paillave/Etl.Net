using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.StreamNodes
{
    public class DistinctSortedArgs<TIn, TKey>
    {
        public ISortedStream<TIn, TKey> InputStream { get; set; }
    }
    public class DistinctSortedStreamNode<TIn, TKey> : StreamNodeBase<TIn, IKeyedStream<TIn, TKey>, DistinctSortedArgs<TIn, TKey>>
    {
        public DistinctSortedStreamNode(string name, DistinctSortedArgs<TIn, TKey> args) : base(name, args)
        {
        }

        protected override IKeyedStream<TIn, TKey> CreateOutputStream(DistinctSortedArgs<TIn, TKey> args)
        {
            return base.CreateKeyedStream(args.InputStream.Observable.DistinctUntilChanged(args.InputStream.SortDefinition), args.InputStream.SortDefinition);
        }
    }
}
