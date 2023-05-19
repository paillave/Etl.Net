using System;

namespace Paillave.Etl.Core
{
    public class DummyDependencyResolver : IDependencyResolver
    {
        public object Resolve(Type type) => default;
        public object Resolve(string key, Type type) => default;
        public T Resolve<T>() where T : class => default;
        public T Resolve<T>(string key) where T : class => default;

        public object Resolve(Type type, string key) => default;

        public bool TryResolve(Type type, string key, out object resolved)
        {
            resolved = default;
            return false;
        }

        public bool TryResolve<T>(out T resolved) where T : class
        {
            resolved = default;
            return false;
        }

        public bool TryResolve<T>(string key, out T resolved) where T : class
        {
            resolved = default;
            return false;
        }

        public bool TryResolve(Type type, out object resolved)
        {
            resolved = default;
            return false;
        }
    }
}