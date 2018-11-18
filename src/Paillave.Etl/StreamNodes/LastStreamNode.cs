using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class LastArgs<TOut>
    {
        public IStream<TOut> Input { get; set; }
    }
    public class LastStreamNode<TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, LastArgs<TOut>>
    {
        public LastStreamNode(string name, LastArgs<TOut> args) : base(name, args)
        {
        }

        protected override ISingleStream<TOut> CreateOutputStream(LastArgs<TOut> args)
        {
            return base.CreateSingleStream(args.Input.Observable.Last());
        }
    }
}
