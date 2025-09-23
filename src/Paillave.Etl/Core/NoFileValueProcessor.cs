using System;
using System.Threading;

namespace Paillave.Etl.Core
{
    public class NoFileValueProcessor(string code) : IFileValueProcessor
    {
        public string Code { get; } = code;
        public ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public void Process(IFileValue fileValue, Action<IFileValue> push, CancellationToken cancellationToken)
        {
            throw new Exception($"{Code}: this file value provider does not exist");
        }
        public void Test() { }
    }
}