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
using System.Reflection;
using System.Collections.Generic;
using Paillave.Etl.EntityFrameworkCore.BulkSave;
using Paillave.Etl.EntityFrameworkCore.EfSave;
using System.Linq.Expressions;
using FundProcess.Pms.Imports.StreamTypes.Config;

namespace FundProcess.Pms.ImportsTests
{
    public class UnitTest1
    {
        private readonly DataAccess.DatabaseContext _databaseContext;

        private int CreateTestManCo(DbContextOptions<DataAccess.DatabaseContext> options)
        {
            // return 12;
            using (var ctx = new DataAccess.DatabaseContext(options, TenantContext.Empty))
            {
                var manCo = ctx.Set<ManCo>().FirstOrDefault(i => i.RegistrationNumber == "TEST");
                if (manCo == null)
                {
                    manCo = new ManCo
                    {
                        RegistrationNumber = "TEST",
                        Name = "my manco"
                    };
                    ctx.Add(manCo);
                    ctx.SaveChanges();
                }
                return manCo.Id;
            }
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
        public void Tempo()
        {
            //List<SubFund> elts = Enumerable
            //    .Range(700, 100)
            //    .Select(i => new SubFund
            //    {
            //        Id = i,
            //        InternalCode = $"code{i}",
            //        Name = $"name{i}",
            //        CurrencyIso = $"CU{i % 10}"
            //    })
            //    .Union(Enumerable
            //    .Range(300000, 100)
            //    .Select(i => new SubFund
            //    {
            //        InternalCode = $"code{i}",
            //        Name = $"name{i}",
            //        CurrencyIso = $"CU{i % 10}"
            //    }))
            //    .ToList();

            var elt = new Sicav
            {
                Id = 2,
                Name = $"name222"
            };

            List<Sicav> elts = new List<Sicav>();
            elts.Add(elt);
            //Stopwatch sw = new Stopwatch();
            _databaseContext.EfSave(elts, i => new { i.Id });
        }
        [Fact]
        public void RbcImportTest()
        {
            StreamProcessRunner.CreateAndExecuteAsync(
                new RbcImportFilesConfigCtx
                {
                    //InputFilesRootFolderPath = @"C:\Users\sroyer\Downloads\RBC",
                    InputFilesRootFolderPath = @"C:\Users\paill\Desktop\rbc",
                    NavFileFileNamePattern = "*NAVPUBLTEXTRACT*.csv",
                    PositionFileFileNamePattern = "*PORTFVALEXTRACT*.csv",
                    DbContext = _databaseContext
                },
                RbcJobs.FullInitialImport,
                traceStream => traceStream
                    .Where("remove verbose", i => i.Content.Level != TraceLevel.Verbose)
                    .ThroughAction("trace", i => Debug.WriteLine(i))
            ).Wait();
        }
        [Fact]
        public void RbcImportAccountTest()
        {
            StreamProcessRunner.CreateAndExecuteAsync(
                new RbcImportAccountFilesConfigCtx
                {
                    //InputFilesRootFolderPath = @"C:\Users\sroyer\Downloads\RBC",
                    InputFilesRootFolderPath = @"C:\Users\paill\Desktop\rbc",
                    AccountFileNamePattern = "*Shareholders*.xlsx",
                    AccountPositionFileNamePattern = "*Position*.xlsx",
                    DbContext = _databaseContext
                },
                RbcAccountJobs.FullInitialImport,
                traceStream => traceStream
                    .Where("remove verbose", i => i.Content.Level != TraceLevel.Verbose)
                    .ThroughAction("trace", i => Debug.WriteLine(i))
            ).Wait();
        }
        [Fact]
        public void EfaImportTest()
        {
            StreamProcessRunner.CreateAndExecuteAsync(
                new EfaImportFilesConfigCtx
                {
                    //InputFilesRootFolderPath = @"C:\Users\sroyer\Downloads\RBC",
                    InputFilesRootFolderPath = @"C:\Users\paill\Desktop\efa",
                    NavFileFileNamePattern = "ffnav1_*.csv",
                    PositionFileFileNamePattern = "ffpos1_*.csv",
                    DbContext = _databaseContext
                },
                EfaJobs.FullInitialImport,
                traceStream => traceStream
                    .Where("remove verbose", i => i.Content.Level != TraceLevel.Verbose)
                    .ThroughAction("trace", i => Debug.WriteLine(i))
            ).Wait();
        }
        [Fact]
        public void BdlImportTest()
        {
            StreamProcessRunner.CreateAndExecuteAsync(
                new BdlImportFilesConfigCtx
                {
                    InputFilesRootFolderPath = @"C:\Users\paill\Desktop\bdl",
                    FileNamePattern = "*.xml",
                    //FileNamePattern = "test.txt",
                    //FileNamePattern = "shelter#aggr_iis_sftp162789960.xml",
                    DbContext = _databaseContext
                },
                BdlJobs.FullInitialImport
                , traceStream =>
                {
                    traceStream.Where("to debug console", i => i.Content.Level != TraceLevel.Verbose).ThroughAction("", i => Debug.WriteLine(i));
                    //traceStream.KeepLastTracesPerNode().ThroughAction("trace", ProcessExecutionSummary);
                }
            ).Wait();
        }

        private static void ProcessExecutionSummary(Dictionary<string, List<TraceEvent>> eventSummary)
        {

        }
    }
}
