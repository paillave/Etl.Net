using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core
{
    public interface INodeContext
    {
        IEnumerable<string> NodeNamePath { get; }
        string TypeName { get; }
    }
}
