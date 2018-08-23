using Paillave.Etl.Core.StreamNodesOld;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.NodeOutputsOld
{
    public class KeyedNodeOutputError<TNode, TOut, TIn> : IKeyedNodeOutputError<TOut, TIn> where TNode : IKeyedStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TIn>>
    {
        public KeyedNodeOutputError(TNode node)
        {
            this.Output = node.Output;
            this.Error = node.Error;
        }
        public IKeyedStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }
    public class KeyedNodeOutputError<TNode, TOut, TIn1, TIn2> : IKeyedNodeOutputError<TOut, TIn1, TIn2> where TNode : IKeyedStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TIn1,TIn2>>
    {
        public KeyedNodeOutputError(TNode node)
        {
            this.Output = node.Output;
            this.Error = node.Error;
        }
        public IKeyedStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn1,TIn2>> Error { get; }
    }
}
