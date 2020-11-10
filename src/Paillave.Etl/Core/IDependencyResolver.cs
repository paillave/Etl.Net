using System;

namespace Paillave.Etl.Core
{
    public interface IDependencyResolver
    {
        T Resolve<T>();
        T Resolve<T>(string key);
        object Resolve(Type type);
    }
}