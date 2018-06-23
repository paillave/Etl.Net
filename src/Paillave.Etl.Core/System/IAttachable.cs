using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public interface IAttachable : IDisposable
    {
        IObservable<ProcessTrace> Trace { get; }
    }
}
