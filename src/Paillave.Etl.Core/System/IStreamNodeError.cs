using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public interface IStreamNodeError<TRow>
    {
        IStream<TRow> Error { get; }
    }
}
