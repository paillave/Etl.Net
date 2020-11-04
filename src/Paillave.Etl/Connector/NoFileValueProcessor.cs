using System;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.Connector
{
    public class NoFileValueProcessor : IFileValueProcessor
    {
        public NoFileValueProcessor(string code) => (Code) = (code);
        public string Code { get; }
        public ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public void Process(IFileValue fileValue, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            throw new Exception($"{Code}: this file value provider does not exist");
        }
        public void Test() { }
    }
}