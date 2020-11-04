using System;

namespace Paillave.Etl.Core
{
    public class DummyDependencyResolver : IDependencyResolver
    {
        public object Resolve(Type type) => default;
        public object Resolve(string key, Type type) => default;
        public T Resolve<T>() => default;
        public T Resolve<T>(string key) => default;
    }
}