using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public class NodeOutputError<TOut, TIn>
    {
        public NodeOutputError(IStream<TOut> output, IStream<ErrorRow<TIn>> error)
        {
            this.Output = output;
            this.Error = error;
        }
        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }
    public class SortedNodeOutputError<TOut, TIn>
    {
        public SortedNodeOutputError(ISortedStream<TOut> output, IStream<ErrorRow<TIn>> error)
        {
            this.Output = output;
            this.Error = error;
        }
        public ISortedStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }
    public class NodeOutputError<TOut, TIn1, TIn2>
    {
        public NodeOutputError(IStream<TOut> output, IStream<ErrorRow<TIn1, TIn2>> error)
        {
            this.Output = output;
            this.Error = error;
        }
        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn1, TIn2>> Error { get; }
    }
}
