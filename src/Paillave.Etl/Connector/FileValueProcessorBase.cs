using System;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.Connector
{
    public abstract class FileValueProcessorBase<TConnectionParameters, TProcessorParameters> : IFileValueProcessor
    {
        public string Code { get; }
        public abstract ProcessImpact PerformanceImpact { get; }
        public abstract ProcessImpact MemoryFootPrint { get; }
        protected string ConnectionName { get; }
        protected string Name { get; }
        private readonly TConnectionParameters _connectionParameters;
        private readonly TProcessorParameters _processorParameters;

        protected FileValueProcessorBase(string code, string name, string connectionName, TConnectionParameters connectionParameters, TProcessorParameters processorParameters)
        {
            Code = code;
            ConnectionName = connectionName;
            Name = name;
            _connectionParameters = connectionParameters;
            _processorParameters = processorParameters;
        }

        public void Process(IFileValue fileValue, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver)
            => Process(fileValue, _connectionParameters, _processorParameters, push, cancellationToken, resolver);
        protected abstract void Process(IFileValue fileValue, TConnectionParameters connectionParameters, TProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver);
        public void Test() => Test(_connectionParameters, _processorParameters);
        protected abstract void Test(TConnectionParameters connectionParameters, TProcessorParameters processorParameters);
    }
}