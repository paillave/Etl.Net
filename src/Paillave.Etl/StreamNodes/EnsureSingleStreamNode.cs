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
    public class EnsureSingleArgs<T>
    {
        public IStream<T> Input { get; set; }
    }
    public class EnsureSingleStreamNode<TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, EnsureSingleArgs<TOut>>
    {
        public EnsureSingleStreamNode(string name, EnsureSingleArgs<TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISingleStream<TOut> CreateOutputStream(EnsureSingleArgs<TOut> args)
        {
            return base.CreateSingleStream(args.Input.Observable.Map((i, idx) =>
            {
                if (idx > 0) throw new Exception($"{this.NodeName}: There are more than one element in the stream");
                else return i;
            }));
        }
    }
}
