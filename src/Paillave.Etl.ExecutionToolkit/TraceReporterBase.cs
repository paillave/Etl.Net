using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.ExecutionToolkit
{
    public class TraceReporterBase : ITraceReporter
    {
        public virtual void Dispose() { }

        protected virtual void HandleTrace(TraceEvent traceEvent)
        {
            switch (traceEvent.Content)
            {
                case CounterSummaryStreamTraceContent counterSummary:
                    this.HandleCounterSummary(traceEvent, counterSummary);
                    break;
                case RowProcessStreamTraceContent rowProcess:
                    this.HandleRowProcess(traceEvent, rowProcess);
                    break;
                case UnhandledExceptionStreamTraceContent unhandledException:
                    this.HandleUnhandledException(traceEvent, unhandledException);
                    break;
            }
        }
        protected virtual void HandleCounterSummary(TraceEvent traceEvent, CounterSummaryStreamTraceContent counterSummary) { }
        protected virtual void HandleRowProcess(TraceEvent traceEvent, RowProcessStreamTraceContent rowProcess) { }
        protected virtual void HandleUnhandledException(TraceEvent traceEvent, UnhandledExceptionStreamTraceContent rowProcess) { }
        public virtual void Initialize(JobDefinitionStructure jobDefinitionStructure) { }

        public void TraceProcessDefinition<TConfig>(IStream<TraceEvent> traceStream, ISingleStream<TConfig> configStream)
            => traceStream.Observable.Do(HandleTrace);
    }
}