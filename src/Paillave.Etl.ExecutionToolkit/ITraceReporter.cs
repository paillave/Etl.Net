using System;
using Paillave.Etl.Core;

namespace Paillave.Etl.ExecutionToolkit
{
    public interface ITraceReporter : IDisposable
    {
        void HandleTrace(TraceEvent traceEvent);
        void Initialize(JobDefinitionStructure jobDefinitionStructure);
    }
}