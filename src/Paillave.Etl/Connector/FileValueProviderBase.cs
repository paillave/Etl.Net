using System;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.Connector
{
    public abstract class FileValueProviderBase<TConnectionParameters, TProviderParameters> : IFileValueProvider
    {
        public string Code { get; }
        public abstract ProcessImpact PerformanceImpact { get; }
        public abstract ProcessImpact MemoryFootPrint { get; }
        protected string ConnectionName { get; }
        protected string Name { get; }
        private readonly TConnectionParameters _connectionParameters;
        private readonly TProviderParameters _providerParameters;

        protected FileValueProviderBase(string code, string name, string connectionName, TConnectionParameters connectionParameters, TProviderParameters providerParameters)
        {
            Code = code;
            ConnectionName = connectionName;
            Name = name;
            _connectionParameters = connectionParameters;
            _providerParameters = providerParameters;
        }

        public void Provide(Action<IFileValue> pushFileValue, CancellationToken cancellationToken, IDependencyResolver resolver)
            => Provide(pushFileValue, _connectionParameters, _providerParameters, cancellationToken, resolver);
        protected abstract void Provide(Action<IFileValue> pushFileValue, TConnectionParameters connectionParameters, TProviderParameters providerParameters, CancellationToken cancellationToken, IDependencyResolver resolver);
        public void Test() => Test(_connectionParameters, _providerParameters);
        protected abstract void Test(TConnectionParameters connectionParameters, TProviderParameters providerParameters);
    }
}