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
    public class EnsureKeyedArgs<T, TKey>
    {
        public IStream<T> Input { get; set; }
        public SortDefinition<T, TKey> SortDefinition { get; set; }
    }
    public class EnsureKeyedStreamNode<TOut, TKey> : StreamNodeBase<TOut, IKeyedStream<TOut, TKey>, EnsureKeyedArgs<TOut, TKey>>
    {
        public EnsureKeyedStreamNode(string name, EnsureKeyedArgs<TOut, TKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IKeyedStream<TOut, TKey> CreateOutputStream(EnsureKeyedArgs<TOut, TKey> args)
        {
            return base.CreateKeyedStream(args.Input.Observable.ExceptionOnUnsorted(args.SortDefinition, true), args.SortDefinition);
        }
    }
}
