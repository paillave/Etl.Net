using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core
{
    public class JobPool : IDisposable
    {
        private object _lock = new object();
        private bool _isStopped = false;

        private Queue<Action> _actionQueue = new Queue<Action>();
        private int _delayBetweenCall = 0;

        public JobPool(int delayBetweenCall = 0)
        {
            _delayBetweenCall = delayBetweenCall;
            Task.Run(() => BackgroundProcess());
        }
        private System.Threading.EventWaitHandle _mtxWaitNewProcess = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);

        private void BackgroundProcess()
        {
            while (!_isStopped)
            {
                lock (_lock)
                {
                    while (_actionQueue.Count > 0)
                    {
                        _actionQueue.Dequeue()();
                        if (_delayBetweenCall != 0)
                            System.Threading.Thread.Sleep(_delayBetweenCall);
                    }
                }
                _mtxWaitNewProcess.WaitOne();
            }
        }
        public Task ExecuteAsync(Action action)
        {
            var tsc = new TaskCompletionSource<object>();
            lock (_lock)
            {
                _actionQueue.Enqueue(() =>
                {
                    try
                    {
                        action();
                        tsc.SetResult(new object());
                    }
                    catch (Exception ex)
                    {
                        tsc.SetException(ex);
                    }
                });
                _mtxWaitNewProcess.Set();
            }
            return tsc.Task;
        }
        public Task<T> ExecuteAsync<T>(Func<T> action)
        {
            var tsc = new TaskCompletionSource<T>();
            lock (_lock)
            {
                _actionQueue.Enqueue(() =>
                {
                    try
                    {
                        tsc.SetResult(action());
                    }
                    catch (Exception ex)
                    {
                        tsc.SetException(ex);
                    }
                });
                _mtxWaitNewProcess.Set();
            }
            return tsc.Task;
        }
        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _isStopped = true;
                    _mtxWaitNewProcess.Set();
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
