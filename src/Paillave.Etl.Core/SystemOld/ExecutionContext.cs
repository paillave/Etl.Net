using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.SystemOld
{
    public class ExecutionContext<TConfig>
    {
        public Guid ExecutionId { get; private set; }

        public ExecutionContext()
        {
            this.ExecutionId = Guid.NewGuid();
        }
        public Task StartAsync(TConfig config)
        {
            //return Task.WhenAll(this._sourceStreamNodes.Select(startable => Task.Run(() => startable.Start())));
        }
        private class StreamTracer : ITracer
        {
            private Guid _executionContextId;
            public StreamTracer(Guid executionContextId)
            {
                this._executionContextId = executionContextId;
            }

            public void OnNextExceptionProcessTrace(ExceptionProcessTrace processTrace)
            {
                throw new NotImplementedException();
            }

            public void OnNextProcessTrace(ProcessTrace processTrace)
            {
                throw new NotImplementedException();
            }
        }
    }
}
