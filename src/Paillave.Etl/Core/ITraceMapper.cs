using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core
{
    public interface ITraceMapper
    {
        //void Trace(ITraceContent content);
        //void AddTraceObservable(IPushObservable<ITraceContent> traceObservable);
        ITraceMapper GetSubTraceMapper(INodeContext nodeContext);
        TraceEvent MapToTrace(ITraceContent content);
    }
}
