using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core
{
    public interface INodeContext
    {
        string NodeName{ get; }
        string TypeName { get; }
    }
}
