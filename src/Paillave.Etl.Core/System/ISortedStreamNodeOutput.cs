using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public interface ISortedStreamNodeOutput<TRow>
    {
        ISortedStream<TRow> Output { get; }
    }
}
