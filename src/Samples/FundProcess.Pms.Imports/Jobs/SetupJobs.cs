using System;
using FundProcess.Pms.Imports.StreamTypes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Extensions;
using Paillave.Etl.TextFile.Extensions;
using Paillave.Etl.EntityFrameworkCore.Extensions;
using FundProcess.Pms.Imports.FileDefinitions;
using Paillave.Etl.EntityFrameworkCore.StreamNodes;
using FundProcess.Pms.DataAccess.Enums;
using FundProcess.Pms.DataAccess;
using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using Paillave.Etl.StreamNodes;
using System.Linq;
using FundProcess.Pms.Imports.StreamTypes.Config;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.Jobs
{
    public class SetupJobs
    {
        public static void ImportSicavSetupNoCtx(ISingleStream<SetupSicavImportFilesConfigNoCtx> configStream)
        {
            ImportSicavSetup(configStream.Select("create config with Ctx",
                c => new SetupSicavImportFilesConfigCtx
                {
                    InputFilesRootFolderPath = c.InputFilesRootFolderPath,
                    FileNamePattern = c.FileNamePattern,
                    DbContext = new DatabaseContext(
                        new DbContextOptionsBuilder<DatabaseContext>().UseSqlServer(new SqlConnectionStringBuilder
                        {
                            IntegratedSecurity = true,
                            DataSource = c.Server,
                            InitialCatalog = c.Database,
                            MultipleActiveResultSets = true
                        }.ConnectionString).Options,
                        new TenantContext(c.EntityId, c.EntityGroupId))
                }));
        }
        public static void ImportSicavSetup(ISingleStream<SetupSicavImportFilesConfigCtx> configStream)
        {
            var dbCnxStream = configStream
                .Select("get dbcnx", i => i.DbContext, true);

            var sicavFileStream = configStream
                .CrossApplyFolderFiles("get all sicav files", i => i.InputFilesRootFolderPath, i => i.FileNamePattern, true)
                .CrossApplyTextFile("parse sicav file", new SetupBdlSicavFileDefinition());

            var sicavStream = sicavFileStream
                .Distinct("distinct sicav", i => i.SicavCode)
                .Select("create sicav", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new Sicav { InternalCode = i.SicavCode, Name = i.SicavName }))
                .ThroughEntityFrameworkCore("save sicav", dbCnxStream, i => i.InternalCode);
        }
    }
}
