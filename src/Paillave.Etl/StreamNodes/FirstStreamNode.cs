using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class FirstArgs<TOut>
    {
        public IStream<TOut> Input { get; set; }
    }
    public class FirstStreamNode<TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, FirstArgs<TOut>>
    {
        public FirstStreamNode(string name, FirstArgs<TOut> args) : base(name, args)
        {
        }

        protected override ISingleStream<TOut> CreateOutputStream(FirstArgs<TOut> args)
        {
            return base.CreateSingleStream(args.Input.Observable.First());
        }
    }
}
