using System;
using System.Threading;

namespace Paillave.Etl.Core
{
    public abstract class FileValueProviderBase<TConnectionParameters, TProviderParameters> : IFileValueProvider, IValuesProvider<object, IFileValue>
    {
        public string Code { get; }
        public abstract ProcessImpact PerformanceImpact { get; }
        public abstract ProcessImpact MemoryFootPrint { get; }
        protected string ConnectionName { get; }
        protected string Name { get; }

        public virtual string TypeName => this.GetType().Name;

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

        public void Provide(Action<IFileValue> pushFileValue, CancellationToken cancellationToken, IExecutionContext context)
            => Provide(pushFileValue, _connectionParameters, _providerParameters, cancellationToken, context);
        protected abstract void Provide(Action<IFileValue> pushFileValue, TConnectionParameters connectionParameters, TProviderParameters providerParameters, CancellationToken cancellationToken, IExecutionContext context);
        public void Test() => Test(_connectionParameters, _providerParameters);
        protected abstract void Test(TConnectionParameters connectionParameters, TProviderParameters providerParameters);
        public void PushValues(object input, Action<IFileValue> push, CancellationToken cancellationToken, IExecutionContext context)
            => Provide(push, cancellationToken, context);
    }
}