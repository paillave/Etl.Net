using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core
{
    public class JobPoolDispatcher
    {
        private object _sync = new object();
        private IDictionary<object, JobPool> _jobPoolDictionary = new Dictionary<object, JobPool>();
        public void Invoke(object threadOwner, Action action)
        {
            JobPool jobPool;
            lock (_sync)
            {
                if (!_jobPoolDictionary.TryGetValue(threadOwner, out jobPool))
                {
                    jobPool = new JobPool();
                    _jobPoolDictionary[threadOwner] = jobPool;
                }
            }
            jobPool.Execute(action);
        }
    }
}