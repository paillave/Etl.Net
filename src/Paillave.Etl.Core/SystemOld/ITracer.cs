using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.SystemOld
{
    public interface ITracer
    {
        void OnNextExceptionProcessTrace(ExceptionProcessTrace processTrace);
        void OnNextProcessTrace(ProcessTrace processTrace);
    }
}
