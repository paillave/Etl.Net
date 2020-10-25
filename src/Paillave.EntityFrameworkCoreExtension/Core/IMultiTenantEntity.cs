using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.EntityFrameworkCoreExtension.Core
{
    public interface IMultiTenantEntity
    {
        int TenantId { get; set; }
    }
}
