using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.SystemOld
{
    public class ProcessTrace
    {
        public IEnumerable<StreamNodeBase> StreamNodeSources { get; private set; }
        public string Message { get; private set; }
        public TraceLevel Level { get; private set; }

        public ProcessTrace(IEnumerable<string> sourceNodeName, string message, TraceLevel level = TraceLevel.Info)
        {
            this.Level = level;
            this.Message = message;
            this.SourceNodeName = sourceNodeName;
        }
        public override string ToString()
        {
            return $"[{string.Join("].[", this.SourceNodeName)}]:({this.Level})-{this.Message}";
        }
    }
}
