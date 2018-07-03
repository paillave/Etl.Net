using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public interface IKeyedStreamNodeOutput<TRow>
    {
        IKeyedStream<TRow> Output { get; }
    }
}
