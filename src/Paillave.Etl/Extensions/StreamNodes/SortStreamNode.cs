using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class SortArgs<T, TKey>
    {
        public IStream<T> Input { get; set; }
        public SortDefinition<T, TKey> SortDefinition { get; set; }
    }
    public class SortStreamNode<TOut, TKey>(string name, SortArgs<TOut, TKey> args) : StreamNodeBase<TOut, ISortedStream<TOut, TKey>, SortArgs<TOut, TKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;

        protected override ISortedStream<TOut, TKey> CreateOutputStream(SortArgs<TOut, TKey> args)=>
             base.CreateSortedStream(args.Input.Observable.ToList().FlatMap((i, ct) =>
            {
                i.Sort(args.SortDefinition);
                return PushObservable.FromEnumerable(i, ct);
            }), args.SortDefinition);
        
    }
}
