using System.Collections.Concurrent;

namespace Paillave.Etl.Core
{
    public class LimitedQueue<T> : ConcurrentQueue<T>
    {
        public int Limit { get; }

        public LimitedQueue(int limit) : base()
        {
            Limit = limit;
        }

        public new void Enqueue(T item)
        {
            while (Count >= Limit)
            {
                TryDequeue(out T _);
            }
            base.Enqueue(item);
        }
    }
}
