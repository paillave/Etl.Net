using System.Collections.Concurrent;

namespace Paillave.Etl.Core
{
    public class FixedQueue<T> : ConcurrentQueue<T>
    {
        private int _limit;
        private object lockObject = new object();
        public FixedQueue(int limit) => _limit = limit;
        public new void Enqueue(T item)
        {
            lock (lockObject)
            {
                base.Enqueue(item);
                while (base.Count > _limit && base.TryDequeue(out T overflow)) ;
            }
        }
    }
}