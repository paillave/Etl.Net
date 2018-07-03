using Paillave.Etl.Core.System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public class DataStreamSourceNode : StreamNodeBase, IConfigurable<Stream>, IStreamNodeOutput<string>
    {
        private Stream _stream;

        public IStream<string> Output => throw new global::System.NotImplementedException();

        public void Configure(Stream stream)
        {
            this._stream = stream;
        }

        public override void Start()
        {
            try
            {
                using (var sr = new StreamReader(this.InputDataStream))
                    while (!sr.EndOfStream)
                        this.OnNextOutput(sr.ReadLine());
            }
            catch (Exception ex)
            {
                base.Tracer.OnNextExceptionProcessTrace(new DataStreamReadExceptionProcessTrace(base.NodeNamePath, ex));
            }
            this.OnCompleted();
        }
    }
    public class DataStreamReadExceptionProcessTrace : ExceptionProcessTrace
    {
        public DataStreamReadExceptionProcessTrace(IEnumerable<string> sourceNodeName, Exception exception) : base(sourceNodeName, exception)
        {
        }
    }
}
