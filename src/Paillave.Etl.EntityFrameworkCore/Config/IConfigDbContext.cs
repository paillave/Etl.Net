using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.EntityFrameworkCore.Config
{
    public interface IConfigDbContext<TCtx> where TCtx : DbContext
    {
        TCtx DbContext { get; }
    }
}
