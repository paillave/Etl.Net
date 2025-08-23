using System;
using System.IO;
using System.Threading;

namespace Paillave.Etl.Core
{
    public class NoFileValueProvider : IFileValueProvider
    {
        public NoFileValueProvider(string code) => Code = code;
        public string Code { get; }
        public ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public void Provide(object input, Action<IFileValue, FileReference> pushFileValue, CancellationToken cancellationToken)
        {
            throw new Exception($"{Code}: this file value provider does not exist");
        }

        public IFileValue Provide(string name, string serialization)
        {
            var stream = new MemoryStream(serialization != null ? System.Text.Encoding.UTF8.GetBytes(serialization) : []);
            return new InMemoryFileValue<NoSourceFileValueMetadata>(new MemoryStream(), name, new NoSourceFileValueMetadata("") { ConnectorCode = Code });
        }

        // public IFileValue Provide(string serialization)
        // {
        //     throw new Exception($"{Code}: this file value provider does not exist");
        // }

        public void Test() { }
    }
}