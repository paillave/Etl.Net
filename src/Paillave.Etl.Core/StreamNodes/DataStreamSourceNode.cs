using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public class DataStreamSourceNode : SourceStreamNodeBase<string>
    {
        public DataStreamSourceNode(ExecutionContextBase traceContext, string name, IEnumerable<string> parentsName = null) : base(traceContext, name, parentsName)
        {
        }

        public Stream InputDataStream { get; set; }

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
                base.Context.OnNextExceptionProcessTrace(new DataStreamReadExceptionProcessTrace(base.NodeNamePath, ex));
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
