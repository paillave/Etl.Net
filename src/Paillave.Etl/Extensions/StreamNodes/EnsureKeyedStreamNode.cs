using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class EnsureKeyedArgs<T, TKey>
    {
        public IStream<T> Input { get; set; }
        public SortDefinition<T, TKey> SortDefinition { get; set; }
    }
    public class EnsureKeyedStreamNode<TOut, TKey>(string name, EnsureKeyedArgs<TOut, TKey> args) : StreamNodeBase<TOut, IKeyedStream<TOut, TKey>, EnsureKeyedArgs<TOut, TKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IKeyedStream<TOut, TKey> CreateOutputStream(EnsureKeyedArgs<TOut, TKey> args) =>
            base.CreateKeyedStream(args.Input.Observable.ExceptionOnUnsorted(args.SortDefinition, true), args.SortDefinition);
    }
}
