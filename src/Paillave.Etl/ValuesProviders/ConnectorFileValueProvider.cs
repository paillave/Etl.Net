using System;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;

namespace Paillave.Etl.ValuesProviders
{
    public class ConnectorFileValueProvider<TIn> : IValuesProvider<TIn, IFileValue>
    {
        private readonly IFileValueProvider _fileValueProvider;
        public ConnectorFileValueProvider(IFileValueProvider fileValueProvider) => _fileValueProvider = fileValueProvider;
        public string TypeName => $"ConnectorFilesSource_{_fileValueProvider.Code}";
        public ProcessImpact PerformanceImpact => this._fileValueProvider.PerformanceImpact;
        public ProcessImpact MemoryFootPrint => this._fileValueProvider.MemoryFootPrint;
        public void PushValues(TIn input, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker) => this._fileValueProvider.Provide(push, cancellationToken, resolver, invoker);
    }
}