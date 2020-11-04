using System;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.Connector
{
    public class NoFileValueProvider : IFileValueProvider
    {
        public NoFileValueProvider(string code) => (Code) = (code);
        public string Code { get; }
        public ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public void Provide(Action<IFileValue> pushFileValue, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            throw new Exception($"{Code}: this file value provider does not exist");
        }
        public void Test() { }
    }
}