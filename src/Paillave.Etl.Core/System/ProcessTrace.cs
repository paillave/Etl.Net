using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public class ProcessTrace
    {
        public string StreamOperatorName { get; private set; }
        public string Message { get; private set; }
        public TraceLevel Level { get; private set; }

        public ProcessTrace(string streamOperatorName, string message, TraceLevel level = TraceLevel.Info)
        {
            this.Level = level;
            this.Message = message;
            this.StreamOperatorName = streamOperatorName;
        }
        public override string ToString()
        {
            return $"{this.StreamOperatorName}:[{this.Level}]-{this.Message}";
        }
    }
}
