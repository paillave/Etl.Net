using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public interface INodeContext
    {
        IEnumerable<string> NodeNamePath { get; }
        string TypeName { get; }
    }
}
