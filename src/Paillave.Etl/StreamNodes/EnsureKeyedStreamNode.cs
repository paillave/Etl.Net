using Paillave.Etl.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.StreamNodesOld;

namespace Paillave.Etl.StreamNodes
{
    public class EnsureKeyedStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, IEnumerable<SortCriteria<TIn>>>, IKeyedStreamNodeOutput<TIn>
    {
        public IKeyedStream<TIn> Output { get; }

        public EnsureKeyedStreamNode(IStream<TIn> input, string name, IEnumerable<SortCriteria<TIn>> arguments)
            : base(input, name, arguments)
        {
            this.Output = base.CreateKeyedStream(nameof(Output), input.Observable, arguments);
        }
    }
}
