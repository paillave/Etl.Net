using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Debugger
{
    public class EtlTraceEventPusher<TConfig> : IEtlTraceEventPusher
    {
        private class Listening : IDisposable
        {
            private bool disposedValue = false;
            private readonly EtlTraceEventPusher<TConfig> _cls;
            private readonly Action<TraceEvent> _listener;

            public Listening(EtlTraceEventPusher<TConfig> cls, Action<TraceEvent> listener)
            {
                this._cls = cls;
                this._listener = listener;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    disposedValue = true;
                    if (_cls._listeners.Contains(_listener))
                        _cls._listeners.Remove(_listener);
                }
            }
            public void Dispose()
            {
                Dispose(true);
            }
        }

        private void PushEvent(TraceEvent traceEvent)
        {
            foreach (var item in _listeners)
                item(traceEvent);
        }

        private List<Action<TraceEvent>> _listeners = new List<Action<TraceEvent>>();
        private readonly StreamProcessRunner<TConfig> _streamProcessRunner;
        private readonly TConfig _config;

        public Task<ExecutionStatus> ResultTask { get; private set; }

        public EtlTraceEventPusher(StreamProcessRunner<TConfig> streamProcessRunner, TConfig config)
        {
            this._streamProcessRunner = streamProcessRunner;
            this._config = config;
        }

        public IDisposable Listen(Action<TraceEvent> listener)
        {
            _listeners.Add(listener);
            return new Listening(this, listener);
        }
        public void Start()
        {
            this.ResultTask = _streamProcessRunner.ExecuteWithNoFaultAsync(_config, i => i.Observable.Do(PushEvent));
        }
    }
}