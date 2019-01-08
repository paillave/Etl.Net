using System;
using Xunit;
using Paillave.Etl.Core;
using Paillave.Etl;
using Paillave.Etl.Extensions;
using FundProcess.Pms.Imports.StreamTypes;
using FundProcess.Pms.Imports;
using FundProcess.Pms.Imports.Jobs;
using Microsoft.EntityFrameworkCore;
using FundProcess.Pms.DataAccess;
using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;
using System.Diagnostics;
using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using Paillave.Etl.StreamNodes;
using System.Linq;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.Reactive.Core;

namespace FundProcess.Pms.ImportsTests
{
    public class UnitTest1
    {
        private readonly DataAccess.DatabaseContext _databaseContext;

        private int CreateTestManCo(DbContextOptions<DataAccess.DatabaseContext> options)
        {
            return 12;
            //using (var ctx = new DataAccess.DatabaseContext(options, TenantContext.Empty))
            //{
            //    var manCo = new ManCo
            //    {
            //        Name = "my manco"
            //    };
            //    ctx.Add(manCo);
            //    ctx.SaveChanges();
            //    return manCo.Id;
            //}
        }
        public UnitTest1()
        {
            var options = new DbContextOptionsBuilder<DataAccess.DatabaseContext>()
                .UseSqlServer("Server=localhost;Database=FundProcess;Trusted_Connection=True;MultipleActiveResultSets=True").Options;
            //.UseInMemoryDatabase("inmemorydb").Options;
            var tenantContext = new TenantContext(this.CreateTestManCo(options), 0);
            _databaseContext = new DataAccess.DatabaseContext(options, tenantContext);
        }
        [Fact]
        public void Test1()
        {
            StreamProcessRunner.CreateAndExecuteAsync(
                new ImportFilesConfig
                {
                    InputFilesRootFolderPath = @"C:\Users\paill\Desktop\rbc",
                    DbCtx = _databaseContext
                },
                ImportFiles.DefineRbcImportProcess,
                traceStream => traceStream
                    .Where("remove verbose", i => i.Content.Level != TraceLevel.Verbose)
                    .ThroughAction("trace", i => Debug.WriteLine(i))
            ).Wait();
            var subFunds = _databaseContext.Set<SubFund>().ToListAsync().Result;
        }
    }
}
