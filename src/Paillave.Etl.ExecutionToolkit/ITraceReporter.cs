using System;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl.ExecutionToolkit
{
    public interface ITraceReporter : IDisposable
    {
        void TraceProcessDefinition<TConfig>(IStream<TraceEvent> traceStream, ISingleStream<TConfig> configStream);
        void Initialize(JobDefinitionStructure jobDefinitionStructure);
    }
}