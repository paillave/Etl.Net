using Paillave.Etl.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.StreamTypes.Config
{
    public interface IConfigMultiTenantDatabaseServer: IConfigDatabaseServer
    {
        int EntityId { get; }
        int EntityGroupId { get; }
    }
}
