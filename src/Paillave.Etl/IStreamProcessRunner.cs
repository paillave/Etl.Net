using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl
{
    public interface IStreamProcessRunner
    {
        Task<ExecutionStatus> ExecuteWithNoFaultAsync(object config, Action<IStream<TraceEvent>> traceProcessDefinition = null);
        JobDefinitionStructure GetDefinitionStructure();
    }
}