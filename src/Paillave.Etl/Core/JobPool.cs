using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paillave.Etl.Core;

public abstract class JobPoolBase(int delayBetweenCall = 0) : IDisposable
{
    private readonly object _lock = new();
    // Volatile: Dispose reads and writes it without holding _lock.
    private volatile bool _isStopped = false;
    private static readonly TimeSpan ShutdownCheckInterval = TimeSpan.FromSeconds(1);

    private readonly Queue<Action> _actionQueue = new();
    private readonly int _delayBetweenCall = delayBetweenCall;

    // Monitor.Wait on _lock rather than an EventWaitHandle: the handle was a kernel object created
    // once per pool and never disposed - Dispose only signalled it - so every job handed a batch of
    // handles to the finalizer queue. Waiting on the lock we already hold needs no handle at all, and
    // it closes the lost-wakeup window the old code had between leaving the lock and calling WaitOne.
    protected void BackgroundProcess()
    {
        lock (_lock)
        {
            while (true)
            {
                while (_actionQueue.Count > 0)
                {
                    _actionQueue.Dequeue()();
                    if (_delayBetweenCall != 0)
                        System.Threading.Thread.Sleep(_delayBetweenCall);
                }
                if (_isStopped) return;
                // Releases _lock while waiting, reacquires it on wake, so producers can enqueue.
                // The timeout only covers one race: Dispose can set _isStopped just after the check
                // above, and its TryEnter then fails because we still hold the lock, so the pulse is
                // lost and an untimed wait would never end - the very thread leak this is fixing.
                // Producers cannot lose a pulse, they signal while holding the lock.
                System.Threading.Monitor.Wait(_lock, ShutdownCheckInterval);
            }
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
            System.Threading.Monitor.Pulse(_lock);
        }
        return tsc.Task;
    }
    public Task ExecuteAsync(Func<Task> actionAsync)
    {
        var tsc = new TaskCompletionSource<object>();
        lock (_lock)
        {
            _actionQueue.Enqueue(() =>
            {
                try
                {
                    actionAsync().GetAwaiter().GetResult();
                    tsc.SetResult(new object());
                }
                catch (Exception ex)
                {
                    tsc.SetException(ex);
                }
            });
            System.Threading.Monitor.Pulse(_lock);
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
            System.Threading.Monitor.Pulse(_lock);
        }
        return tsc.Task;
    }
    public Task<T> ExecuteAsync<T>(Func<Task<T>> functionAsync)
    {
        var tsc = new TaskCompletionSource<T>();
        lock (_lock)
        {
            _actionQueue.Enqueue(() =>
            {
                try
                {
                    tsc.SetResult(functionAsync().GetAwaiter().GetResult());
                }
                catch (Exception ex)
                {
                    tsc.SetException(ex);
                }
            });
            System.Threading.Monitor.Pulse(_lock);
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
                // Deliberately non blocking. If the lock is free the pool is idle in Monitor.Wait and
                // needs a pulse to notice the flag; if it is taken the pool is draining its queue and
                // will re-read _isStopped as soon as it is done, so no pulse is needed. Taking the
                // lock unconditionally here would instead make Dispose wait out the running job.
                if (System.Threading.Monitor.TryEnter(_lock))
                {
                    try { System.Threading.Monitor.PulseAll(_lock); }
                    finally { System.Threading.Monitor.Exit(_lock); }
                }
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
    // A dedicated background thread, not Task.Run: BackgroundProcess spends its life blocked, and
    // blocking a ThreadPool worker indefinitely makes the pool inject replacement threads (roughly one
    // per second) whose stacks are working set the GC cannot account for.
    public JobPool(int delayBetweenCall = 0) : base(delayBetweenCall)
    {
        var thread = new System.Threading.Thread(BackgroundProcess)
        {
            IsBackground = true,
            Name = "EtlJobPool",
        };
        thread.Start();
    }
}
public class InThreadJobPool : JobPoolBase
{
    public InThreadJobPool(int delayBetweenCall = 0) : base(delayBetweenCall) { }
    public void Listen(Task task)
    {
        task.ContinueWith(t => this.Dispose());
        base.BackgroundProcess();
    }
}
