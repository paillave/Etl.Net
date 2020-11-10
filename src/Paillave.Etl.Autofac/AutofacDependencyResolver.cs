using System;
using System.Collections.Generic;
using Autofac;
using Paillave.Etl.Core;

namespace Paillave.Etl.Autofac
{
    public class AutofacDependencyResolver : IDependencyResolver
    {
        private SimpleDependencyResolver _dependencyResolver = new SimpleDependencyResolver();
        private readonly IComponentContext _componentContext;
        public AutofacDependencyResolver(IComponentContext componentContext) => (_componentContext) = (componentContext);
        public object Resolve(Type type)
        {
            var res = _dependencyResolver.Resolve(type);
            if (res != null) return res;
            res = _componentContext.Resolve(type);
            _dependencyResolver.Register(type, res);
            return res;
        }
        public T Resolve<T>()
        {
            var res = _dependencyResolver.Resolve<T>();
            if (res != null) return res;
            res = _componentContext.Resolve<T>();
            _dependencyResolver.Register<T>(res);
            return res;
        }
        public T Resolve<T>(string key)
        {
            var res = _dependencyResolver.Resolve<T>(key);
            if (res != null) return res;
            res = _componentContext.ResolveKeyed<T>(key);
            _dependencyResolver.Register<T>(res, key);
            return res;
        }
    }
}