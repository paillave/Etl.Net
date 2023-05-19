using System;
using System.Collections.Generic;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class CompositeDependencyResolver : IDependencyResolver
    {
        private readonly List<IDependencyResolver> _dependencyResolvers = new List<IDependencyResolver>();
        private object _lock = new object();
        public CompositeDependencyResolver AddResolver(IDependencyResolver dependencyResolver)
        {
            _dependencyResolvers.Add(dependencyResolver);
            return this;
        }

        public T Resolve<T>() where T : class
        {
            lock (_lock)
            {
                T resolved = default;
                foreach (var dependencyResolver in _dependencyResolvers)
                    if (dependencyResolver.TryResolve<T>(out resolved))
                        return resolved;
            }
            return default;
        }

        public T Resolve<T>(string key) where T : class
        {
            lock (_lock)
            {
                T resolved = default;
                foreach (var dependencyResolver in _dependencyResolvers)
                    if (dependencyResolver.TryResolve<T>(key, out resolved))
                        return resolved;
            }
            return default;
        }

        public object Resolve(Type type)
        {
            lock (_lock)
            {
                object resolved = default;
                foreach (var dependencyResolver in _dependencyResolvers)
                    if (dependencyResolver.TryResolve(type, out resolved))
                        return resolved;
            }
            return default;
        }

        public bool TryResolve<T>(out T resolved) where T : class
        {
            lock (_lock)
            {
                resolved = default;
                foreach (var dependencyResolver in _dependencyResolvers)
                    if (dependencyResolver.TryResolve<T>(out resolved))
                        return true;
            }
            return false;
        }

        public bool TryResolve<T>(string key, out T resolved) where T : class
        {
            lock (_lock)
            {
                resolved = default;
                foreach (var dependencyResolver in _dependencyResolvers)
                    if (dependencyResolver.TryResolve<T>(key, out resolved))
                        return true;
            }
            return false;
        }

        public object Resolve(Type type, string key)
        {
            lock (_lock)
            {
                object resolved = default;
                foreach (var dependencyResolver in _dependencyResolvers)
                    if (dependencyResolver.TryResolve(type, key, out resolved))
                        return resolved;
            }
            return default;
        }

        public bool TryResolve(Type type, string key, out object resolved)
        {
            lock (_lock)
            {
                resolved = default;
                foreach (var dependencyResolver in _dependencyResolvers)
                    if (dependencyResolver.TryResolve(type, key, out resolved))
                        return true;
            }
            return false;
        }

        public bool TryResolve(Type type, out object resolved)
        {
            lock (_lock)
            {
                resolved = default;
                foreach (var dependencyResolver in _dependencyResolvers)
                    if (dependencyResolver.TryResolve(type, out resolved))
                        return true;

            }
            return false;
        }
    }
}
