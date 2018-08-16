using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.NodeOutputs
{
    public class NodeOutputError<TNode, TOut, TIn> : INodeOutputError<TOut, TIn> where TNode : IStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TIn>>
    {
        public NodeOutputError(TNode node)
        {
            this.Output = node.Output;
            this.Error = node.Error;
        }
        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }
    public class NodeOutputError<TNode, TOut, TIn1, TIn2> : INodeOutputError<TOut, TIn1, TIn2> where TNode : IStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TIn1, TIn2>>
    {
        public NodeOutputError(TNode node)
        {
            this.Output = node.Output;
            this.Error = node.Error;
        }
        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn1, TIn2>> Error { get; }
    }
}
