using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public class DataStreamSourceNode : SourceStreamNodeBase<string, Exception>
    {
        public DataStreamSourceNode(ExecutionContextBase traceContext, string name) : base(traceContext, name)
        {
        }

        public Stream InputDataStream { get; set; }

        public override void Start()
        {
            using (var sr = new StreamReader(this.InputDataStream))
                while (!sr.EndOfStream)
                    this.OnNextOutput(sr.ReadLine());
        }
    }
}
