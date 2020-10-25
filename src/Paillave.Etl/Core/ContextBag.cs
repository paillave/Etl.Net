using System;
using System.Collections.Generic;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class ContextBag
    {
        private object _lock = new object();
        private IDictionary<string, object> _dictionary = new Dictionary<string, object>();
        public ContextBag() { }
        public T Get<T>(string key, Func<T> creator)
        {
            lock (_lock)
            {
                if (this._dictionary.TryGetValue(key, out var ret)) return (T)ret;
                ret = creator();
                this._dictionary[key] = ret;
                return (T)ret;
            }
        }
        public T Get<T>(Func<T> creator)
        {
            lock (_lock)
            {
                T ret = this._dictionary.Values.OfType<T>().FirstOrDefault();
                if (ret != null) return ret;
                ret = creator();
                this._dictionary[typeof(T).Name] = ret;
                return (T)ret;
            }
        }
        public T Get<T>(string key)
        {
            lock (_lock)
            {
                if (this._dictionary.TryGetValue(key, out var ret)) return (T)ret;
                return default;
            }
        }
        public T Get<T>()
        {
            lock (_lock)
            {
                T ret = this._dictionary.Values.OfType<T>().FirstOrDefault();
                if (ret != null) return ret;
                return default;
            }
        }
        public object Get(Type type)
        {
            lock (_lock)
            {
                return this._dictionary.Values.FirstOrDefault(v => v.GetType() == type);
            }
        }
    }
}
