using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.NodeOutputs
{
    public class SortedNodeOutputError<TNode, TOut, TIn> : ISortedNodeOutputError<TOut, TIn> where TNode : ISortedStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TIn>>
    {
        public SortedNodeOutputError(TNode node)
        {
            this.Output = node.Output;
            this.Error = node.Error;
        }
        public ISortedStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }
    public class SortedNodeOutputError<TNode, TOut, TIn1, TIn2> : ISortedNodeOutputError<TOut, TIn1, TIn2> where TNode : ISortedStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TIn1, TIn2>>
    {
        public SortedNodeOutputError(TNode node)
        {
            this.Output = node.Output;
            this.Error = node.Error;
        }
        public ISortedStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn1, TIn2>> Error { get; }
    }
}
