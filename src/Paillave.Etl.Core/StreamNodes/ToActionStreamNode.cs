using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.Etl.Core.StreamNodes;

namespace Paillave.Etl.StreamNodes
{
    public class ToActionStreamNode<TIn> : AwaitableStreamNodeBase<IStream<TIn>, TIn, Action<TIn>>
    {
        public ToActionStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, Action<TIn> arguments) : base(input, name, parentNodeNamePath, arguments)
        {
        }
        protected override void ProcessValue(TIn value)
        {
            this.Arguments(value);
        }
    }
}
