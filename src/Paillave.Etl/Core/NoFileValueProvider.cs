using System;
using System.IO;
using System.Threading;
using System.Text.Json;

namespace Paillave.Etl.Core
{
    public class NoFileValueProvider(string code) : IFileValueProvider
    {
        public string Code { get; } = code;
        public ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public void Provide(object input, Action<IFileValue, FileReference> pushFileValue, CancellationToken cancellationToken)
        {
            throw new Exception($"{Code}: this file value provider does not exist");
        }
        private class FileSpecificData
        {
            public byte[] Content { get; set; }
            public string FileName { get; set; }
        }

        public IFileValue Provide(string fileSpecific)
        {
            var fileSpecificData = JsonSerializer.Deserialize<FileSpecificData>(fileSpecific) ?? throw new Exception("Invalid file specific");
            var stream = new MemoryStream(fileSpecificData.Content);
            return new InMemoryFileValue(stream, fileSpecificData.FileName);
        }

        public void Test() { }
    }
}