using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Config;

public interface IConfigConnectionString
{
    string ConnectionString { get; }
}
