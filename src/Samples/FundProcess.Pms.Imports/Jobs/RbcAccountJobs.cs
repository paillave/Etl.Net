using System;
using FundProcess.Pms.Imports.StreamTypes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Extensions;
using Paillave.Etl.ExcelFile.Extensions;
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
using FundProcess.Pms.Imports.StreamTypes.InputOutput;

namespace FundProcess.Pms.Imports.Jobs
{
    public class RbcAccountJobs
    {
        public static void FullInitialImportProcessWithNoCtx(ISingleStream<RbcImportAccountFilesConfigNoCtx> configStream)
        {
            FullInitialImport(configStream.Select("create config with Ctx",
                c => new RbcImportAccountFilesConfigCtx
                {
                    InputFilesRootFolderPath = c.InputFilesRootFolderPath,
                    AccountFileNamePattern = c.AccountFileNamePattern,
                    AccountPositionFileNamePattern = c.AccountPositionFileNamePattern,
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
        public static void FullInitialImport(ISingleStream<RbcImportAccountFilesConfigCtx> configStream)
        {
            var dbCnxStream = configStream
                .Select("get dbcnx", i => i.DbContext, true);

            var accountStream = configStream
                .CrossApplyFolderFiles("get all account files", i => i.InputFilesRootFolderPath, i => i.AccountFileNamePattern, true)
                .CrossApplyExcelSheets("get all account excel sheets", i => i.Name)
                .CrossApplyExcelRows("get account excel content", new RbcAccountFileDefinition())
                .Distinct("distinct account on account number", i => i.RegisterAccount)
                .Select("create account", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new RegisterAccount
                {
                    Number = i.RegisterAccount,
                    DealerTaCode = i.Dealer,
                    Name = i.AccountName,
                    SortName = i.AccountSortName
                }))
                .ThroughEntityFrameworkCore("save account", dbCnxStream, i => new { i.Number, i.BelongsToEntityId });

            var accountPositionStream = configStream
                .CrossApplyFolderFiles("get all account position files", i => i.InputFilesRootFolderPath, i => i.AccountPositionFileNamePattern, true)
                .CrossApplyExcelSheets("get all account position excel sheets", i => i.Name)
                .CrossApplyExcelRows("get account position excel content", new RbcAccountPositionFileDefinition())
                .Distinct("exclude duplicate positions", i => new { i.AccountNumber, i.HoldingDate, i.Isin })
                .EntityFrameworkCoreLookup("lookup for shareclass", dbCnxStream, (RbcAccountPositionFile f, ShareClass e) => f.Isin == e.Isin, (l, r) => new { FromFile = l, ShareClass = r })
                .Where("exclude positions with no matching shareclass", i => i.ShareClass != null)
                .Lookup("lookup for account", accountStream, i => i.FromFile.AccountNumber, i => i.Number, (l, r) => new { l.FromFile, l.ShareClass, Account = r })
                .Where("exclude positions with no matching account", i => i.Account != null)
                .Select("create account position", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new RegisterPosition
                {
                    HoldingDate = i.FromFile.HoldingDate,
                    MarketValueInSubFundCcy = i.FromFile.SharesBalance,
                    NbShares = i.FromFile.Assets,
                    RegisterAccountId = i.Account.Id
                }))
                .ThroughEntityFrameworkCore("save account position", dbCnxStream, i => new { i.RegisterAccountId, i.HoldingDate, i.ShareClassId, i.BelongsToEntityId });
        }
    }
}
