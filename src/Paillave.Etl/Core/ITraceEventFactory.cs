using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core
{
    public interface ITraceEventFactory
    {
        //void Trace(ITraceContent content);
        //void AddTraceObservable(IPushObservable<ITraceContent> traceObservable);
        ITraceEventFactory GetSubTraceMapper(INodeContext nodeContext);
        TraceEvent CreateTraceEvent(ITraceContent content, int sequenceId);
    }
}
