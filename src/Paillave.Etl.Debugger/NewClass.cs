using System;
using System.Collections.Generic;
using System.Timers;

namespace Paillave.Etl.Debugger
{
    public class NewClass
    {
        private class Listening : IDisposable
        {
            private bool disposedValue = false;
            private readonly NewClass _cls;
            private readonly Action<string> _listener;

            public Listening(NewClass cls, Action<string> listener)
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
        private List<Action<string>> _listeners = new List<Action<string>>();
        private Timer _timer = new Timer();
        private int _counter = 0;
        public NewClass()
        {
            _timer.Interval = 1000;
            _timer.Elapsed += TimerElapsed;
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var item in _listeners)
                item($"counter: {_counter++}s");
        }
        public IDisposable Listen(Action<string> listener)
        {
            _listeners.Add(listener);
            return new Listening(this, listener);
        }
        public void Start()
        {
            _timer.Start();
        }
    }
}