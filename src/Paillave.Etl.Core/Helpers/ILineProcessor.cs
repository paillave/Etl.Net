using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.Helpers
{
    public interface ILineProcessor<TDest>
    {
        TDest Parse(string[] values);
    }
}
