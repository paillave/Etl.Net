using System;

namespace Paillave.Etl.Core
{
    public class DummyDependencyResolver : IDependencyResolver
    {
        public object Get(Type type) => default;
        public object Get(string key, Type type) => default;
        public T Resolve<T>() => default;
        public T Resolve<T>(string key) => default;
    }
}