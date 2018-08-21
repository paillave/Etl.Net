using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public interface IKeyedStreamNodeOutput<TRow>
    {
        IKeyedStream<TRow> Output { get; }
    }
}
