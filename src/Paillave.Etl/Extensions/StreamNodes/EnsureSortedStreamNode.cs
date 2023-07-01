using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class EnsureSortedArgs<T, TKey>
    {
        public IStream<T> Input { get; set; }
        public SortDefinition<T, TKey> SortDefinition { get; set; }
    }
    public class EnsureSortedStreamNode<TOut, TKey> : StreamNodeBase<TOut, ISortedStream<TOut, TKey>, EnsureSortedArgs<TOut, TKey>>
    {
        public EnsureSortedStreamNode(string name, EnsureSortedArgs<TOut, TKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISortedStream<TOut, TKey> CreateOutputStream(EnsureSortedArgs<TOut, TKey> args)
        {
            return base.CreateSortedStream(args.Input.Observable.ExceptionOnUnsorted(args.SortDefinition, false), args.SortDefinition);
        }
    }
}
