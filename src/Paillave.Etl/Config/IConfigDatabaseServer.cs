using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Config
{
    public interface IConfigDatabaseServer
    {
        string Database { get; }
        string Server { get; }
    }
}
