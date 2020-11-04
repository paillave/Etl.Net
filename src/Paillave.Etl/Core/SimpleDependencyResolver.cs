using System;
using System.Collections.Generic;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class SimpleDependencyResolver : IDependencyResolver
    {
        private object _lock = new object();
        private IDictionary<string, object> _dictionary = new Dictionary<string, object>();
        public SimpleDependencyResolver Register<T>(T instance, string key = null)
        {
            if (key == null)
                key = typeof(T).Name;
            this._dictionary[typeof(T).Name] = instance;
            return this;
        }
        public SimpleDependencyResolver() { }
        public T Resolve<T>(string key, Func<T> creator)
        {
            lock (_lock)
            {
                if (this._dictionary.TryGetValue(key, out var ret)) return (T)ret;
                ret = creator();
                this._dictionary[key] = ret;
                return (T)ret;
            }
        }
        public T Resolve<T>(Func<T> creator)
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
        public T Resolve<T>(string key)
        {
            lock (_lock)
            {
                if (this._dictionary.TryGetValue(key, out var ret)) return (T)ret;
                return default;
            }
        }
        public T Resolve<T>()
        {
            lock (_lock)
            {
                T ret = this._dictionary.Values.OfType<T>().FirstOrDefault();
                if (ret != null) return ret;
                return default;
            }
        }
        public object Resolve(Type type)
        {
            lock (_lock)
            {
                return this._dictionary.Values.FirstOrDefault(v => v.GetType() == type);
            }
        }

        public object Resolve(string key, Type type)
        {
            lock (_lock)
            {
                return this._dictionary.FirstOrDefault(v => v.Value.GetType() == type && v.Key == key).Value;
            }
        }
    }
}
