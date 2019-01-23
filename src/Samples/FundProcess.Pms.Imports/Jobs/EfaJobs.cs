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

namespace FundProcess.Pms.Imports.Jobs
{
    public class EfaJobs
    {
        public static void FullInitialImportProcessWithNoCtx(ISingleStream<ImportFilesConfigNoCtx> configStream)
        {
            FullInitialImport(configStream.Select("create config with Ctx", c =>
            {
                var csb = new SqlConnectionStringBuilder();
                csb.IntegratedSecurity = true;
                csb.DataSource = c.Server;
                csb.InitialCatalog = c.Database;
                csb.MultipleActiveResultSets = true;
                return new ImportFilesConfig
                {
                    InputFilesRootFolderPath = c.InputFilesRootFolderPath,
                    DbCtx = new DatabaseContext(
                        new DbContextOptionsBuilder<DataAccess.DatabaseContext>().UseSqlServer(csb.ConnectionString).Options,
                        new TenantContext(c.EntityId, c.EntityGroupId))
                };
            }));
        }
        public static void FullInitialImport(ISingleStream<ImportFilesConfig> configStream)
        {
            var navFileStream = configStream
                .CrossApplyFolderFiles("get all Nav files", i => i.InputFilesRootFolderPath, i => i.NavFileFileNamePattern, true)
                .CrossApplyTextFile("parse nav file", new EfaNavFileDefinition());

            var posFileStream = configStream
                .CrossApplyFolderFiles("get all position files", i => i.InputFilesRootFolderPath, i => i.PositionFileFileNamePattern, true)
                .CrossApplyTextFile("parse position file", new EfaPositionFileDefinition(), i => i.Name);

            var dbCnxStream = configStream
                .Select("get dbcnx", i => i.DbCtx, true);

            var sicavStream = posFileStream
                .Distinct("distinct sicavs", i => i.FundCode)
                .Select("create sicav", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new Sicav
                {
                    InternalCode = i.FundCode,
                    Name = i.FundLongName
                }))
                .ThroughEntityFrameworkCore("save sicav", dbCnxStream, i => new { i.InternalCode, i.BelongsToEntityId });

            var subFundStream = posFileStream
                .Distinct("distinct funds", i => i.SubFundCode)
                .Lookup("lookup related sicav", sicavStream, i => i.FundCode, i => i.InternalCode, (l, r) => new { FromFile = l, Sicav = r })
                .Select("create fund", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new SubFund
                {
                    InternalCode = i.FromFile.SubFundCode,
                    SicavId = i.Sicav.Id,
                    Name = i.FromFile.SubFundName,
                    CurrencyIso = i.FromFile.SubFundCurrency
                }))
                .ThroughEntityFrameworkCore("save sub fund", dbCnxStream, i => new { i.InternalCode, i.BelongsToEntityId });

            var shareClassStream = navFileStream
                .Distinct("distinct share classes", i => new { i.SubFundCode, i.ShareCode })
                .Lookup("lookup related sub fund", subFundStream, i => i.SubFundCode, i => i.InternalCode, (l, r) => new { FromFile = l, SubFund = r })
                .Select("create share class", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new ShareClass
                {
                    InternalCode = $"{i.SubFund.InternalCode}_{i.FromFile.ShareCode}",
                    Name = $"{i.SubFund.Name}_{i.FromFile.ShareCode}",
                    CurrencyIso = i.FromFile.ShareCurrency,
                    Isin = i.FromFile.IsinCode,
                    SubFundId = i.SubFund.Id
                }))
                .ThroughEntityFrameworkCore("save share class", dbCnxStream, i => new { i.InternalCode, i.BelongsToEntityId });

            navFileStream
                .Distinct("distinct nav", i => new { i.NavDate, i.SubFundCode, i.ShareCode })
                .Unpivot("unpivot share class historical values", FieldsToUnpivot.Create(
                         (EfaNavFile i) => new { Type = HistoricalValueType.TNA, Value = i.TotalNetAsset },
                         (EfaNavFile i) => new { Type = HistoricalValueType.MKT, Value = i.NavPerShare },
                         (EfaNavFile i) => new { Type = HistoricalValueType.NBS, Value = i.NetAssetShareType }
                    ),
                    (i, j) => new
                    {
                        Date = i.NavDate,
                        j.Type,
                        ShareClassInternalCode = $"{i.SubFundCode}_{i.ShareCode}",
                        j.Value,
                    })
                .Lookup("get hv related share class", shareClassStream, i => i.ShareClassInternalCode, i => i.InternalCode, (l, r) => new { FromFile = l, FromDb = r })
                .Select("create share class hv", dbCnxStream, (i, dbCnx) => dbCnx.UpdateEntityForMultiTenancy(new HistoricalValue
                {
                    SecurityId = i.FromDb.Id,
                    Date = i.FromFile.Date,
                    Type = i.FromFile.Type,
                    Value = i.FromFile.Value
                }))
                .ThroughEntityFrameworkCore("save share class hv", dbCnxStream, i => new { i.Date, i.SecurityId, i.Type, i.BelongsToEntityId });
        }
    }
}
