using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paillave.Etl.Core;

public abstract class JobPoolBase(int delayBetweenCall = 0) : IDisposable
{
private readonly object _lock = new();
private bool _isStopped = false;

private readonly Queue<Action> _actionQueue = new();
private readonly int _delayBetweenCall = delayBetweenCall;
    private readonly System.Threading.EventWaitHandle _mtxWaitNewProcess = new(false, System.Threading.EventResetMode.AutoReset);

protected void BackgroundProcess()
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
public Task ExecuteAsync(Func<Task> actionAsync)
{
    var tsc = new TaskCompletionSource<object>();
    lock (_lock)
    {
        _actionQueue.Enqueue(async () =>
        {
            try
            {
                await actionAsync();
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
public Task<T> ExecuteAsync<T>(Func<T> function)
{
    var tsc = new TaskCompletionSource<T>();
    lock (_lock)
    {
        _actionQueue.Enqueue(() =>
        {
            try
            {
                tsc.SetResult(function());
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
public Task<T> ExecuteAsync<T>(Func<Task<T>> functionAsync)
{
    var tsc = new TaskCompletionSource<T>();
    lock (_lock)
    {
        _actionQueue.Enqueue(async () =>
        {
            try
            {
                tsc.SetResult(await functionAsync());
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

public class JobPool : JobPoolBase
{
    public JobPool(int delayBetweenCall = 0) : base(delayBetweenCall) => Task.Run(() => BackgroundProcess());
}
public class InThreadJobPool(int delayBetweenCall = 0) : JobPoolBase(delayBetweenCall)
{
    public void Listen(Task task)
    {
        task.ContinueWith(t => this.Dispose());
        base.BackgroundProcess();
    }
}
