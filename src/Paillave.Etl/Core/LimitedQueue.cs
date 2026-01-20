using System.Collections.Concurrent;

namespace Paillave.Etl.Core;

public class LimitedQueue<T>(int limit) : ConcurrentQueue<T>()
{
    public int Limit { get; } = limit;

    public new void Enqueue(T item)
    {
        while (Count >= Limit)
        {
            TryDequeue(out T _);
        }
        base.Enqueue(item);
    }
}
