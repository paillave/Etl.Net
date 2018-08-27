using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core
{
    public interface IConfigurable<TConfig>
    {
        void Configure(TConfig config);
    }
}
