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
using FundProcess.Pms.Imports.StreamTypes.InputOutput;

namespace FundProcess.Pms.Imports.Jobs
{
    public class RbcJobs
    {
        public static void FullInitialImportProcessWithNoCtx(ISingleStream<RbcImportFilesConfigNoCtx> configStream)
        {
            FullInitialImport(configStream.Select("create config with Ctx",
                c => new RbcImportFilesConfigCtx
                {
                    InputFilesRootFolderPath = c.InputFilesRootFolderPath,
                    NavFileFileNamePattern = c.NavFileFileNamePattern,
                    PositionFileFileNamePattern = c.PositionFileFileNamePattern,
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
        public static void FullInitialImport(ISingleStream<RbcImportFilesConfigCtx> configStream)
        {
            var dbCnxStream = configStream
                .Select("get dbcnx", i => i.DbContext, true);

            var navFileStream = configStream
                .CrossApplyFolderFiles("get all Nav files", i => i.InputFilesRootFolderPath, i => i.NavFileFileNamePattern, true)
                .CrossApplyTextFile("parse nav file", new RbcNavFileDefinition());

            var subFundStream = navFileStream
                .Distinct("distinct sub funds", i => i.FundCode)
                .Select("create sub funds", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new SubFund
                {
                    InternalCode = i.FundCode,
                    Name = i.FundName,
                    CurrencyIso = i.FundCurrency,
                    IsManaged = true
                }))
                .ThroughEntityFrameworkCore("save sub funds", dbCnxStream, i => new { i.InternalCode, i.BelongsToEntityId });

            var shareClassStream = navFileStream
                .Distinct("distinct share classes", i => new { i.FundCode, i.IsinCode })
                .Lookup("link to related sub fund", subFundStream, i => i.FundCode, i => i.InternalCode, (l, r) => new { FromFile = l, FromDb = r })
                .Select("create share classes", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new ShareClass
                {
                    InternalCode = $"{i.FromFile.FundCode}_{i.FromFile.IsinCode}",
                    Name = $"{i.FromFile.FundName}_{i.FromFile.IsinCode}",
                    CurrencyIso = i.FromFile.Currency,
                    SubFundId = i.FromDb.Id,
                    Isin = i.FromFile.IsinCode
                }))
                .ThroughEntityFrameworkCore("save share classes", dbCnxStream, i => new { i.InternalCode, i.BelongsToEntityId });

            navFileStream
                .Distinct("distinct sub funds and date", i => new { i.FundCode, i.NavDate })
                .Unpivot("unpivot sub funds historical values", FieldsToUnpivot.Create(
                         (RbcNavFile i) => new { Type = HistoricalValueType.TNA, Value = (decimal?)i.FundTotalNetAsset },
                         (RbcNavFile i) => new { Type = HistoricalValueType.RED, Value = i.AmountRedemption },
                         (RbcNavFile i) => new { Type = HistoricalValueType.SUB, Value = i.AmountSubscription }
                    ),
                    (i, j) => new
                    {
                        Date = i.NavDate,
                        j.Type,
                        FundInternalCode = i.FundCode,
                        j.Value,
                    })
            .Where("Exclude empty values from sub fund hv", i => i.Value != null)
            .Lookup("get hv related sub fund", subFundStream, i => i.FundInternalCode, i => i.InternalCode, (l, r) => new { FromFile = l, FromDb = r })
            .Select("create sub fund hv", dbCnxStream, (i, dbCnx) => dbCnx.UpdateEntityForMultiTenancy(new HistoricalValue
            {
                SecurityId = i.FromDb.Id,
                Date = i.FromFile.Date,
                Type = i.FromFile.Type,
                Value = i.FromFile.Value.Value
            }))
            .ThroughEntityFrameworkCore("save sub fund hv", dbCnxStream, i => new { i.Date, i.SecurityId, i.Type, i.BelongsToEntityId });


            navFileStream
                .Unpivot("unpivot share class historical values", FieldsToUnpivot.Create(
                         (RbcNavFile i) => new { Type = HistoricalValueType.TNA, Value = i.TotalNetAsset },
                         (RbcNavFile i) => new { Type = HistoricalValueType.MKT, Value = i.NavPerShare }
                    ),
                    (i, j) => new
                    {
                        Date = i.NavDate,
                        j.Type,
                        ShareClassInternalCode = $"{i.FundCode}_{i.IsinCode}",
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

            var posFileStream = configStream
                .CrossApplyFolderFiles("get all position files", i => i.InputFilesRootFolderPath, i => i.PositionFileFileNamePattern, true)
                .CrossApplyTextFile("parse position file", new RbcPositionFileDefinition(), i => i.Name);

            var compositionStream = posFileStream
                .Distinct("distinct composition for a date", i => new { i.FundCode, i.NavDate })
                .Lookup("get composition sub fund", subFundStream, i => i.FundCode, i => i.InternalCode, (l, r) => new { FromFile = l, SubFund = r })
                .ThroughEntityFrameworkCore("save composition", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new PortfolioComposition
                {
                    Date = i.FromFile.NavDate,
                    PortfolioId = i.SubFund.Id
                }),
                    i => new { i.PortfolioId, i.Date, i.BelongsToEntityId },
                    (i, j) => new
                    {
                        i.FromFile,
                        Composition = j
                    });

            var composingSecuritiesStream = posFileStream
                .Distinct("distinct positions security", i => i.InternalNumber)
                .Select("create security for composition", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(CreateSecurityForComposition(i.InvestmentType, i.Currency, i.IsinCode, i.InstrumentName, i.InternalNumber, i.NextCouponDate)), false, true)
                .ThroughEntityFrameworkCore("save security for composition", dbCnxStream, i => new { i.InternalCode, i.BelongsToEntityId });

            var composingSecurityStream = posFileStream
                .Lookup("lookup for composition security", composingSecuritiesStream, i => i.InternalNumber, i => i.InternalCode, (l, r) => new { FromFile = l, Security = r })
                .Where("exclude composition security with no security", i => i.Security != null)
                .Lookup("get related composition", compositionStream,
                    i => new { i.FromFile.FundCode, i.FromFile.NavDate },
                    i => new { i.FromFile.FundCode, i.FromFile.NavDate },
                    (l, r) => new
                    {
                        l.FromFile.Quantity,
                        l.FromFile.MarketValueInFdCcy,
                        l.FromFile.MarketValueInSecCcy,
                        l.FromFile.PercNav,
                        r.Composition,
                        l.Security
                    })
                .Select("create position", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new Position
                {
                    SecurityId = i.Security.Id,
                    MarketValueInPortfolioCcy = i.MarketValueInFdCcy,
                    MarketValueInSecurityCcy = i.MarketValueInSecCcy,
                    PortfolioCompositionId = i.Composition.Id,
                    Value = i.Quantity,
                    Weight = i.PercNav
                }))
                .ThroughEntityFrameworkCore("save position", dbCnxStream, i => new { i.SecurityId, i.PortfolioCompositionId, i.BelongsToEntityId });
        }
        private static Security CreateSecurityForComposition(string rbcType, string currencyIso, string isin, string name, string internalCode, DateTime? nextCouponDate)
        {
            var splitted = rbcType.Split(':');
            var rbcCode = string.IsNullOrWhiteSpace(rbcType) ? "" : splitted[0].Trim();
            Security security = null;
            switch (rbcCode)
            {
                case "100": // SHARES
                case "102": //
                case "103": //
                case "120": //
                case "117": // REITS
                case "118": // NON G.T. REITS
                case "411": // SICAF
                    security = new Equity();
                    break;
                case "484": // Investment Funds - UCITS- French
                case "485": // Investment Funds - UCITS- European
                    security = new SubFund();
                    break;
                case "200": // STRAIGHT BONDS
                case "201": // FLOATING RATE BONDS
                case "270": // Commercial paper
                case "271": // Certificate of Deposit
                case "202": // "202 : ZERO COUPON BONDS"
                    security = new Bond { NextCouponDate = nextCouponDate };
                    break;
                case "603": // Call/Put
                    var derivativeType = splitted[1].TrimStart();
                    if (derivativeType.StartsWith("option"))
                        security = new Option { Type = OptionType.European };
                    else if (derivativeType.StartsWith("future"))
                        security = new Future();
                    break;
            }
            if (security != null)
            {
                security.InternalCode = internalCode;
                security.CurrencyIso = currencyIso;
                security.Isin = isin;
                security.Name = name;
            }
            return security;
        }
    }
}
