using System;

namespace Paillave.Etl.Core
{
    public interface IDependencyResolver
    {
        T Resolve<T>();
        T Resolve<T>(string key);
        object Get(Type type);
        object Get(string key, Type type);

    }
}