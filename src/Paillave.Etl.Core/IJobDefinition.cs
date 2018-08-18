using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl
{
    public interface IJobDefinition<TConfig>
    {
        void DefineJob(IStream<TConfig> startupStream);
        string Name { get; }
    }
}
