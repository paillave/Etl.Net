using System.Collections.Concurrent;

namespace Paillave.Etl.Core;

public class FixedQueue<T>(int limit) : ConcurrentQueue<T>
{
    private readonly int _limit = limit;
    private readonly object lockObject = new();

    public new void Enqueue(T item)
    {
        lock (lockObject)
        {
            base.Enqueue(item);
            while (base.Count > _limit && base.TryDequeue(out T overflow)) ;
        }
    }
}