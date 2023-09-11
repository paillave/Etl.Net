using System;
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
        public T Resolve<T>() where T : class
        {
            var res = _dependencyResolver.Resolve<T>();
            if (res != null) return res;
            res = _componentContext.Resolve<T>();
            _dependencyResolver.Register<T>(res);
            return res;
        }
        public T Resolve<T>(string key) where T : class
        {
            var res = _dependencyResolver.Resolve<T>(key);
            if (res != null) return res;
            res = _componentContext.ResolveKeyed<T>(key);
            _dependencyResolver.Register<T>(res, key);
            return res;
        }

        public bool TryResolve<T>(out T resolved) where T : class
        {
            resolved = default;
            return _componentContext.TryResolve<T>(out resolved);
        }

        public bool TryResolve<T>(string key, out T resolved) where T : class
        {
            resolved = default;
            return _componentContext.TryResolveKeyed<T>(key, out resolved);
        }

        public bool TryResolve(Type type, out object resolved)
        {
            return _componentContext.TryResolve(type, out resolved);
        }

        public object Resolve(Type type, string key)
        {
            var res = _dependencyResolver.Resolve(type, key);
            if (res != null) return res;
            res = _componentContext.ResolveKeyed(key, type);
            _dependencyResolver.Register(type, res, key);
            return res;
        }

        public bool TryResolve(Type type, string key, out object resolved)
        {
            resolved = default;
            return _componentContext.TryResolveKeyed(key, type, out resolved);
        }
    }
}