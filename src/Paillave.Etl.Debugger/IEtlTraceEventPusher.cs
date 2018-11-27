using System;
using Paillave.Etl.Core;

namespace Paillave.Etl.Debugger
{
    public interface IEtlTraceEventPusher
    {
        IDisposable Listen(Action<TraceEvent> listener);
        void Start();
    }
}