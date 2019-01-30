using System;
using FundProcess.Pms.Imports.StreamTypes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Extensions;
using Paillave.Etl.XmlFile.Extensions;
using Paillave.Etl.TextFile.Extensions;
using Paillave.Etl.EntityFrameworkCore.Extensions;
using Paillave.Etl.StreamNodes;
using FundProcess.Pms.Imports.FileDefinitions;
using FundProcess.Pms.DataAccess.Enums;
using FundProcess.Pms.DataAccess;
using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using FundProcess.Pms.Imports.StreamTypes.Config;
using System.Text.RegularExpressions;
using FundProcess.Pms.Imports.StreamTypes.InputOutput;

namespace FundProcess.Pms.Imports.Jobs
{
    public class BdlJobs
    {
        public static void FullInitialImportProcessWithNoCtx(ISingleStream<BdlImportFilesConfigNoCtx> configStream)
        {
            FullInitialImport(configStream.Select("create config with Ctx",
                c => new BdlImportFilesConfigCtx
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
        public static void FullInitialImport(ISingleStream<BdlImportFilesConfigCtx> configStream)
        {
            var dbCnxStream = configStream
                .Select("get dbcnx", i => i.DbContext, true);

            var nodesStream = configStream
                .CrossApplyFolderFiles("get all Nav files", i => i.InputFilesRootFolderPath, i => i.FileNamePattern, true)
                .CrossApplyXmlFile("parse input file", new BdlFileDefinition());

            var sourceTargetSecuritiesStream = nodesStream
                .Where("list target securities", i => i.NodeDefinitionName == "secbase")
                .Select("cast to target security", i => (BdlSecBaseNode)i.Value)
                .Distinct("distinct target securities", i => i.SecurityCode);

            var sourceSecurityPositionsStream = nodesStream
                .Where("list securities positions", i => i.NodeDefinitionName == "secpos")
                .Select("cast to security position", i => (BdlSecPosNode)i.Value);

            var sourceCashPositionsStream = nodesStream
                .Where("list cash positions", i => i.NodeDefinitionName == "cashpos")
                .Select("cast to cash position", i => (BdlCashposNode)i.Value)
                .ThroughAction("correct the iban if empty", i => { if (string.IsNullOrWhiteSpace(i.Iban)) i.Iban = $"BDL_{i.ContId}_{i.AssetCcy}"; });

            var sourcePortfoliosStream = nodesStream
                .Where("list portfolios", i => i.NodeDefinitionName == "custid")
                .Select("cast to portfolio", i => (BdlCustidNode)i.Value)
                .Distinct("distinct portfolio", i => i.ContId);

            var portfolioStream = sourcePortfoliosStream
                .Select("create portfolio", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new Portfolio
                {
                    InternalCode = i.ContId,
                    Name = $"BDL_{i.ContId}",
                    CurrencyIso = i.DefaultCcy,
                    CountryCode = i.Domicile,
                    IsManaged = true
                }))
                .ThroughEntityFrameworkCore("save portfolio", dbCnxStream, i => new { i.InternalCode, i.BelongsToEntityId });

            var portfolioCompositionFromSecurityPositionStream = sourceSecurityPositionsStream
                .Lookup("get portfolio for security position", portfolioStream, i => i.ContId, i => i.InternalCode, (l, r) => new { FromFile = l, Portfolio = r })
                .Select("create composition from security position", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new PortfolioComposition
                {
                    Date = i.FromFile.AssBalDate,
                    PortfolioId = i.Portfolio.Id
                }));

            var portfolioCompositionFromCashPositionStream = sourceCashPositionsStream
                .Lookup("get portfolio for cash position", portfolioStream, i => i.ContId, i => i.InternalCode, (l, r) => new { FromFile = l, Portfolio = r })
                .Select("create composition from cash position", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new PortfolioComposition
                {
                    Date = i.FromFile.PosBalDate,
                    PortfolioId = i.Portfolio.Id
                }));

            var portfolioCompositionStream = portfolioCompositionFromSecurityPositionStream
                .Union("join portfolio composition from security and from cash", portfolioCompositionFromCashPositionStream)
                .Distinct("distinct portfolio composition", i => new { i.Date, i.PortfolioId })
                .ThroughEntityFrameworkCore("save portfolio composition", dbCnxStream, i => new { i.BelongsToEntityId, i.Date, i.PortfolioId });

            var targetSecurityStream = sourceTargetSecuritiesStream
                .Distinct("distinct target securities", i => i.SecurityCode)
                .Select("create target security", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(CreateTargetSecurity(i.InstrType, i.Isin, i.SecurityCode, i.SecName, i.Domicile, i.InstrCcy, i.ValFreq)))
                .Where("exclude unknown security types", i => i != null)
                .ThroughEntityFrameworkCore("save target security", dbCnxStream, i => new { i.BelongsToEntityId, i.InternalCode });

            var targetCashStream = sourceCashPositionsStream
                .Distinct("distinct target cash", i => i.Iban)
                .Select("create target cash", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new Cash
                {
                    InternalCode = i.Iban,
                    CurrencyIso = i.AssetCcy,
                    Name = $"BDL_{i.Iban}"
                }))
                .ThroughEntityFrameworkCore("save target cash", dbCnxStream, i => new { i.BelongsToEntityId, i.InternalCode });

            var cashPositions = sourceCashPositionsStream
                .Lookup("lookup target cash", targetCashStream, i => i.Iban, i => i.InternalCode, (l, r) => new { FromFile = l, Cash = r })
                .Lookup("lookup cash porfolio", portfolioStream, i => i.FromFile.ContId, i => i.InternalCode, (l, r) => new { l.FromFile, l.Cash, Portfolio = r })
                .Lookup("lookup cash portfolio composition", portfolioCompositionStream,
                    i => new { PortfolioId = i.Portfolio.Id, Date = i.FromFile.PosBalDate },
                    i => new { i.PortfolioId, Date = i.Date.Value },
                    (l, r) => new { l.FromFile, l.Cash, l.Portfolio, PortfolioComposition = r })
                .Select("create cash pos", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new Position
                {
                    PortfolioCompositionId = i.PortfolioComposition.Id,
                    SecurityId = i.Cash.Id,
                    Value = 1,
                    MarketValueInPortfolioCcy = i.FromFile.PosBalRefCcy + i.FromFile.AccrInt
                }))
                .ThroughEntityFrameworkCore("save cash position", dbCnxStream, i => new { i.BelongsToEntityId, i.PortfolioCompositionId, i.SecurityId });

            var targetSecurityPosition = sourceSecurityPositionsStream
                .Lookup("lookup target security", targetSecurityStream, i => i.SecurityCode, i => i.InternalCode, (l, r) => new { FromFile = l, Security = r })
                .Where("ignore positions without target security", i => i.Security != null)
                .Lookup("lookup security porfolio", portfolioStream, i => i.FromFile.ContId, i => i.InternalCode, (l, r) => new { l.FromFile, l.Security, Portfolio = r })
                .Lookup("lookup securityportfolio composition", portfolioCompositionStream,
                    i => new { PortfolioId = i.Portfolio.Id, Date = i.FromFile.AssBalDate },
                    i => new { i.PortfolioId, Date = i.Date.Value },
                    (l, r) => new { l.FromFile, l.Security, l.Portfolio, PortfolioComposition = r })
                .Select("create security pos", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new Position
                {
                    PortfolioCompositionId = i.PortfolioComposition.Id,
                    SecurityId = i.Security.Id,
                    Value = i.FromFile.AssetQty,
                    MarketValueInSecurityCcy = i.FromFile.TotSec,
                    MarketValueInPortfolioCcy = i.FromFile.TotRefCcy
                }))
                .ThroughEntityFrameworkCore("save security position", dbCnxStream, i => new { i.BelongsToEntityId, i.PortfolioCompositionId, i.SecurityId });

        }

        private static Regex _srriRegex = new Regex(@".+[^(][(](?<code>\d{1,2})[)]$", RegexOptions.Compiled);
        private static int? ConvertSrri(string srriText)
        {
            var match = _srriRegex.Match(srriText);
            if (match == null) return null;
            var code = match.Groups["code"].Value;
            if (int.TryParse(code, out var srri)) return srri;
            else return null;
        }

        private static Security CreateTargetSecurity(int instrType, string isin, string securityCode, string name, string domicile, string instrCcy, string valFreq)
        {
            Security security = null;
            switch (instrType)
            {
                case 805:
                case 810:
                case 820:
                case 830:
                case 840:
                case 850:
                case 855:
                case 870:
                case 880:
                case 890:
                case 896:
                    security = new SubFund();
                    break;
                case 201:
                case 202:
                case 910:
                case 911:
                case 993:
                    security = new Equity();
                    break;
                default:
                    break;
            }
            if (security == null) return null;
            security.Name = name;
            security.Isin = isin;
            security.CountryCode = domicile;
            security.CurrencyIso = instrCcy;
            security.InternalCode = securityCode;
            return security;
        }
    }
}
