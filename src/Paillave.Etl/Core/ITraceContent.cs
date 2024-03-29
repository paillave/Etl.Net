﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core
{
    public interface ITraceContent
    {
        string Type { get; }
        TraceLevel Level { get; }
        string Message { get; }
    }
}
