using System;
using System.Collections.Generic;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class SimpleDependencyResolver : IDependencyResolver
    {
        private object _lock = new object();
        private IDictionary<string, object> _namedDictionary = new Dictionary<string, object>();
        private IDictionary<Type, object> _typedDictionary = new Dictionary<Type, object>();
        public SimpleDependencyResolver Register<T>(T instance, string key) => Register(typeof(T), instance, key);
        public SimpleDependencyResolver Register(Type type, object instance, string key)
        {
            lock (_lock)
            {
                this._namedDictionary[key] = instance;
                return this;
            }
        }
        public SimpleDependencyResolver Register<T>(T instance) => Register(typeof(T), instance);
        public SimpleDependencyResolver Register(Type type, object instance)
        {
            lock (_lock)
            {
                this._typedDictionary[type] = instance;
                return this;
            }
        }
        public SimpleDependencyResolver() { }
        public T Resolve<T>(string key, Func<T> creator)
        {
            lock (_lock)
            {
                if (this._namedDictionary.TryGetValue(key, out var ret)) return (T)ret;
                ret = creator();
                this._namedDictionary[key] = ret;
                return (T)ret;
            }
        }
        public T Resolve<T>(Func<T> creator)
        {
            lock (_lock)
            {
                if (this._typedDictionary.TryGetValue(typeof(T), out var ret)) return (T)ret;
                ret = creator();
                this._typedDictionary[typeof(T)] = ret;
                return (T)ret;
            }
        }
        public T Resolve<T>(string key)
        {
            lock (_lock)
            {
                return (T)Resolve(key);
            }
        }
        public T Resolve<T>()
        {
            lock (_lock)
            {
                return (T)Resolve(typeof(T));
            }
        }
        public object Resolve(Type type)
        {
            lock (_lock)
            {
                if (this._typedDictionary.TryGetValue(type, out var ret)) return ret;
                return default;
            }
        }

        public object Resolve(string key)
        {
            lock (_lock)
            {
                if (this._namedDictionary.TryGetValue(key, out var res)) return res;
                return null;
            }
        }
    }
}
