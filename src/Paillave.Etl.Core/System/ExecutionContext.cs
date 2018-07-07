using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public class ExecutionContext<TConfig> : IConfigurable<TConfig>//, IDisposable
    {
        private readonly ISubject<TraceEvent> _traceSubject;
        private readonly ISubject<TConfig> _configSubject;
        public SortedStream<TraceEvent> TraceStream { get; }
        public Stream<TConfig> StartupStream { get; }
        private TConfig _config;
        private IList<IObservable<bool>> _streamsToEnd = new List<IObservable<bool>>();

        public ExecutionContext(string jobName)
        {
            this.JobName = jobName;
            this.ExecutionId = Guid.NewGuid();
            this._traceSubject = new Subject<TraceEvent>();
            this._configSubject = new Subject<TConfig>();
            this.TraceStream = new SortedStream<TraceEvent>(null, new NullExecutionContext(this), null, this._traceSubject, new[] { new SortCriteria<TraceEvent>(i => i.DateTime) });
            var executionContext = new CurrentExecutionContext(this);
            this.StartupStream = new Stream<TConfig>(new Tracer(executionContext, new CurrentExecutionNodeContext(jobName)), executionContext, null, this._configSubject.SingleAsync().Publish().RefCount());
            //this._streamToEnd = this.StartupStream.Observable.Select(i => true);
        }

        public void Configure(TConfig config)
        {
            this._config = config;
        }

        public Guid ExecutionId { get; }

        //private TDataSource CreateDataSource<TDataSource, TRow, TDataSourceConf>(string name, Func<TConfig, TDataSourceConf> getDsConfig) where TDataSource : DataSourceNodeBase<TDataSourceConf, TRow>, new()
        //{
        //    TDataSource ds = new TDataSource();
        //    ds.Initialize(StartupStream.ExecutionContext, name);
        //    ds.SetupStream(this._configSubject.Select(getDsConfig));
        //    return ds;
        //}

        //public IStream<TRow> DataSource<TDataSource, TRow, TDataSourceConf>(string name, Func<TConfig, TDataSourceConf> getDsConfig) where TDataSource : DataSourceNodeBase<TDataSourceConf, TRow>, new()
        //{
        //    return this.CreateDataSource<TDataSource, TRow, TDataSourceConf>(name, getDsConfig).Output;
        //}

        public async Task ExecuteAsync()
        {
            //this._streamsToEnd.Subscribe(_ => { }, () => this._traceSubject.OnCompleted());
            this._configSubject.OnNext(this._config);
            this._configSubject.OnCompleted();
            await Observable.Merge(this._streamsToEnd);
            //await this._streamToEnd.LastOrDefaultAsync();
        }

        public string JobName { get; }

        private void Trace(TraceEvent traceEvent)
        {
            this._traceSubject.OnNext(traceEvent);
        }
        private class CurrentExecutionNodeContext : INodeContext
        {
            private readonly string _jobName;

            public CurrentExecutionNodeContext(string jobName)
            {
                this._jobName = jobName;
            }
            public IEnumerable<string> NodeNamePath => new[] { _jobName };

            public string TypeName => "ExecutionContext";
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

            public void AddObservableToWait<TRow>(IObservable<TRow> observable)
            {
                _localExecutionContext._streamsToEnd.Add(observable.Select(i => true));
            }
        }
        private class NullExecutionContext : IExecutionContext
        {
            private ExecutionContext<TConfig> _localExecutionContext;
            public NullExecutionContext(ExecutionContext<TConfig> localExecutionContext)
            {
                this._localExecutionContext = localExecutionContext;
            }
            public Guid ExecutionId { get; }
            public string JobName { get; }
            public void Trace(TraceEvent traveEvent) { }

            public void AddObservableToWait<TRow>(IObservable<TRow> observable)
            {
                //_localExecutionContext._streamToEnd = _localExecutionContext._streamToEnd.Merge(observable.Select(i => true));
            }
        }

        //#region IDisposable Support
        //private bool disposedValue = false;

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //            this._traceSubject.OnCompleted();
        //        }
        //        disposedValue = true;
        //    }
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //}
        //#endregion
    }
}
