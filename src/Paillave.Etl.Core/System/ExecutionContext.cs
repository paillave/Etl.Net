using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public class ExecutionContext<TConfig> : IDisposable, IConfigurable<TConfig>
    {
        private readonly ISubject<TraceEvent> _traceSubject;
        private readonly ISubject<TConfig> _configSubject;
        public SortedStream<TraceEvent> ProcessTraceContextStream { get; }
        public Stream<TConfig> ConfigStream { get; }
        private TConfig _config;

        public ExecutionContext(string jobName)
        {
            this.JobName = jobName;
            this.ExecutionId = Guid.NewGuid();
            this._traceSubject = new Subject<TraceEvent>();
            this._configSubject = new Subject<TConfig>();
            this.ProcessTraceContextStream = new SortedStream<TraceEvent>(null, new NullExecutionContext(), null, this._traceSubject, new[] { new SortCriteria<TraceEvent>(i => i.DateTime) });
            var executionContext = new CurrentExecutionContext(this);
            this.ConfigStream = new Stream<TConfig>(new Tracer(executionContext, new CurrentExecutionNodeContext()), executionContext, null, this._configSubject);
        }

        public void Configure(TConfig config)
        {
            this._config = config;
        }

        public Guid ExecutionId { get; }

        private TDataSource CreateDataSource<TDataSource, TRow, TDataSourceConf>(string name, Func<TConfig, TDataSourceConf> getDsConfig) where TDataSource : StreamNodeBase, IDataSourceConfigurable<TDataSourceConf, TRow>, new()
        {
            TDataSource ds = new TDataSource();
            ds.Initialize(ConfigStream.ExecutionContext, name);
            ds.Configure(_configSubject.SelectMany(cnfg=>), getDsConfig(this._config));
            return ds;
        }

        public IStream<TRow> DataSource<TDataSource, TRow, TDataSourceConf>(string name, Func<TConfig, TDataSourceConf> getDsConfig) where TDataSource : StreamNodeBase, IConfigurable<TDataSourceConf>, IStreamNodeOutput<TRow>, new()
        {
            return this.CreateDataSource<TDataSource, TRow, TDataSourceConf>(name, getDsConfig).Output;
        }

        public ISortedStream<TRow> SortedDataSource<TDataSource, TRow, TDataSourceConf>(string name, Func<TConfig, TDataSourceConf> getDsConfig) where TDataSource : StreamNodeBase, IConfigurable<TDataSourceConf>, ISortedStreamNodeOutput<TRow>, new()
        {
            return this.CreateDataSource<TDataSource, TRow, TDataSourceConf>(name, getDsConfig).Output;
        }

        public IKeyedStream<TRow> KeyedDataSource<TDataSource, TRow, TDataSourceConf>(string name, Func<TConfig, TDataSourceConf> getDsConfig) where TDataSource : StreamNodeBase, IConfigurable<TDataSourceConf>, IKeyedStreamNodeOutput<TRow>, new()
        {
            return this.CreateDataSource<TDataSource, TRow, TDataSourceConf>(name, getDsConfig).Output;
        }

        public void Start()
        {
            this._configSubject.OnNext(_config);
        }

        public string JobName { get; }

        private void Trace(TraceEvent traceEvent)
        {
            this._traceSubject.OnNext(traceEvent);
        }
        private class CurrentExecutionNodeContext : INodeContext
        {
            public IEnumerable<string> NodeNamePath => new[] { "Root" };

            public string TypeName => "Root";
        }
        private class CurrentExecutionContext : IExecutionContext
        {
            public CurrentExecutionContext(ExecutionContext<TConfig> localExecutionContext)
            {
                this._localExecutionContext = localExecutionContext;
            }
            private ExecutionContext<TConfig> _localExecutionContext;
            public Guid ExecutionId { get { return this._localExecutionContext.ExecutionId; } }

            public string JobName { get { return this._localExecutionContext.JobName; } }

            public void Trace(TraceEvent traceEvent)
            {
                this._localExecutionContext.Trace(traceEvent);
            }
        }
        private class NullExecutionContext : IExecutionContext
        {
            public Guid ExecutionId { get; }
            public string JobName { get; }
            public void Trace(TraceEvent traveEvent) { }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this._traceSubject.OnCompleted();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
