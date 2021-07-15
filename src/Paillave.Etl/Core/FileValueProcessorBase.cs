using System;
using System.Threading;

namespace Paillave.Etl.Core
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

        public void Process(IFileValue fileValue, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
            => Process(fileValue, _connectionParameters, _processorParameters, push, cancellationToken, resolver, invoker);
        protected abstract void Process(IFileValue fileValue, TConnectionParameters connectionParameters, TProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker);
        public void Test() => Test(_connectionParameters, _processorParameters);
        protected abstract void Test(TConnectionParameters connectionParameters, TProcessorParameters processorParameters);
    }
}