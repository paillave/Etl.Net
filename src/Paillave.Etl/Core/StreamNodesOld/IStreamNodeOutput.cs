using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.StreamNodesOld
{
    public interface IStreamNodeOutput<TRow>
    {
        IStream<TRow> Output { get; }
    }
    public interface IStreamNodeOutput<TStream, TRow> where TStream:IStream<TRow>
    {
        TStream Output { get; }
    }
}
