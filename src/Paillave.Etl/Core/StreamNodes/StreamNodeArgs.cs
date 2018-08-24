using System.Collections.Generic;
using System.Linq;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl.Core.StreamNodes
{
    public class StreamNodeArgs
    {
        private IEnumerable<IStream> GetStreams()
        {
            return GetType()
                .GetProperties()
                .Select(propertyInfo => propertyInfo.GetValue(this))
                .OfType<IStream>();
        }
        public List<StreamToNodeLink> GetInputStreamArgumentsLinks(string nodeName)
        {
            return GetStreams()
                .Select(stream => new StreamToNodeLink(stream.SourceNodeName, stream.Name, nodeName))
                .ToList();
        }
        public IExecutionContext GetExecutionContext()
        {
            return this.GetStreams()
                .Select(i => i.ExecutionContext)
                .FirstOrDefault(i => !i.IsTracingContext);
        }
    }
}
