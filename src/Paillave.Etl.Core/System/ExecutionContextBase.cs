using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public abstract class ExecutionContextBase : IDisposable
    {
        public Guid ExecutionId { get; private set; }
        private IList<IDisposable> _disposables;
        private IList<ISourceStreamNode> _startables;
        public virtual void Attach(ISourceStreamNode attachable)
        {
            this._startables.Add(attachable);
            attachable.Trace.Subscribe(this.OnNextProcessTrace);
        }
        public virtual void Attach(IAttachable attachable)
        {
            this._disposables.Add(attachable);
            attachable.Trace.Subscribe(this.OnNextProcessTrace);
        }

        public abstract void OnNextProcessTrace(ProcessTrace processTrace);
        public ExecutionContextBase()
        {
            this._disposables = new List<IDisposable>();
            this._startables = new List<ISourceStreamNode>();
            this.ExecutionId = Guid.NewGuid();
        }
        public Task StartAsync()
        {
            return Task.WhenAll(this._startables.Select(startable => Task.Run(() => startable.Start())));
        }
        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var startable in this._startables) startable.Dispose();
                    foreach (var stream in this._disposables) stream.Dispose();
                }
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
