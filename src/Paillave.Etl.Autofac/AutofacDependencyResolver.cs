using System;
using Autofac;
using Paillave.Etl.Core;

namespace Paillave.Etl.Autofac
{
    public class AutofacDependencyResolver : IDependencyResolver
    {
        private readonly IComponentContext _componentContext;
        public AutofacDependencyResolver(IComponentContext componentContext) => (_componentContext) = (componentContext);
        public object Get(Type type) => _componentContext.Resolve(type);
        public object Get(string key, Type type) => _componentContext.ResolveKeyed(key, type);
        public T Resolve<T>() => _componentContext.Resolve<T>();
        public T Resolve<T>(string key) => _componentContext.ResolveKeyed<T>(key);
    }
}