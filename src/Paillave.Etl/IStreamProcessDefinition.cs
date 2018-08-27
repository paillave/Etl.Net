using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl
{
    public interface IStreamProcessDefinition<TConfig>
    {
        void DefineProcess(IStream<TConfig> rootStream);
        string Name { get; }
    }
}
