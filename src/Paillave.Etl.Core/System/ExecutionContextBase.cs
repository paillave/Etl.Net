using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public abstract class ExecutionContextBase
    {
        public Guid ExecutionId { get; private set; }
        private IList<ISourceStreamNode> _sourceStreamNodes;
        public virtual void Attach(ISourceStreamNode sourceStreamNode)
        {
            this._sourceStreamNodes.Add(sourceStreamNode);
        }

        public abstract void OnNextExceptionProcessTrace(ExceptionProcessTrace processTrace);
        public abstract void OnNextProcessTrace(ProcessTrace processTrace);
        public ExecutionContextBase()
        {
            this._sourceStreamNodes = new List<ISourceStreamNode>();
            this.ExecutionId = Guid.NewGuid();
        }
        public Task StartAsync()
        {
            return Task.WhenAll(this._sourceStreamNodes.Select(startable => Task.Run(() => startable.Start())));
        }
    }
}
