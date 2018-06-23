using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public class NullTraceContext : ExecutionContextBase
    {
        public override void OnNextProcessTrace(ProcessTrace processTrace) { }
    }
}
