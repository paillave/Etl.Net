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

namespace FundProcess.Pms.Imports.Jobs
{
    public class ImportFiles
    {
        private class RbcNavFileContext
        {
            public RbcNavFile FileInput { get; set; }
            public DatabaseContext DbContext { get; set; }
        }
        // private static IStream<RbcNavFileContext> ProcessSubfund(IStream<RbcNavFileContext> inputStream)
        // {
        //     return inputStream;
        // }
        public static void DefineRbcImportProcessWithNoCtx(ISingleStream<ImportFilesConfigNoCtx> configStream)
        {
            DefineRbcImportProcess(configStream.Select("create config with Ctx", c =>
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
        public static void DefineRbcImportProcess(ISingleStream<ImportFilesConfig> configStream)
        {
            var navFileStream = configStream
                .CrossApplyFolderFiles("get all Nav files", i => i.InputFilesRootFolderPath, "*NAVPUBLTEXTRACT*.csv", true)
                .CrossApplyTextFile("parse nav file", new RbcNavFileDefinition());
            //.ThroughAction("console", i => Console.WriteLine(i.FundName));

            // // navFileStream
            // //     .Select("apply dbcontext", dbCnxStream, (f, c) => new RbcNavFileContext { DbContext = c, FileInput = f })
            // //     .GroupBy("group nav file per sub fund", i => i.FileInput.FundCode, ProcessSubfund);


            var dbCnxStream = configStream
                .Select("get dbcnx", i => i.DbCtx, true);

            var subFundStream = navFileStream
                .Distinct("distinct sub funds", i => i.FundCode)
                .Select("create sub funds", i => new SubFund
                {
                    InternalCode = i.FundCode,
                    Name = i.FundName,
                    CurrencyIso = i.FundCurrency,
                })
                .ThroughEntityFrameworkCore("save sub funds", dbCnxStream, (SubFund sf1, SubFund sf2) => sf1.InternalCode == sf2.InternalCode);

            var shareClassStream = navFileStream
                .Distinct("distinct share classes", i => $"{i.FundCode}_{i.IsinCode}")
                .Lookup("link to related sub fund", subFundStream, i => i.FundCode, i => i.InternalCode, (l, r) => new { FromFile = l, FromDb = r })
                .Select("create share classes", i => new ShareClass
                {
                    InternalCode = $"{i.FromFile.FundCode}_{i.FromFile.IsinCode}",
                    Name = $"{i.FromFile.FundName}_{i.FromFile.IsinCode}",
                    CurrencyIso = i.FromFile.Currency,
                    SubFundId = i.FromDb.Id,
                    Isin = i.FromFile.IsinCode
                })
                .ThroughEntityFrameworkCore("save share classes", dbCnxStream, (ShareClass sc1, ShareClass sc2) => sc1.InternalCode == sc2.InternalCode);

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
            .Select("create sub fund hv", dbCnxStream, (i, dbCnx) => dbCnx.UpdateForMultiTenancy(new HistoricalValue
            {
                SecurityId = i.FromDb.Id,
                Date = i.FromFile.Date,
                Type = i.FromFile.Type,
                Value = i.FromFile.Value.Value
            }))
            .ThroughEntityFrameworkCore("save sub fund hv", dbCnxStream);


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
                .Select("create share class hv", dbCnxStream, (i, dbCnx) => dbCnx.UpdateForMultiTenancy(new HistoricalValue
                {
                    SecurityId = i.FromDb.Id,
                    Date = i.FromFile.Date,
                    Type = i.FromFile.Type,
                    Value = i.FromFile.Value
                }))
                .ThroughEntityFrameworkCore("save share class hv", dbCnxStream);

            ////var posFileStream = configStream
            ////    .CrossApplyFolderFiles("get all position files", i => i.InputFilesRootFolderPath, "*PORTFVALEXTRACT*.csv", true)
            ////    .CrossApplyTextFile("parse position file", new RbcPositionFileDefinition(), i => i.Name);

            //// posFileStream
            ////     .Distinct("distinct positions", i => new { i.FundCode, i.IsinCode, i.NavDate })
            ////     .Lookup("get position related sub fund", subFundStream, i => i.FundCode, i => i.InternalCode, (l, r) => new { FromFile = l, FromDb = r })
            ////     .Select("get manco id", dbCnxStream, (l, r) => new { l.FromFile, l.FromDb, ManCoId = r.ManCoId })
            ////     .ThroughEntityFrameworkCore("Save security that composes other security", dbCnxStream, i => i.FromFile., i => new PortfolioComposition { }, i => i);

            //// posFileStream
            ////     .Distinct("distinct positions", i => new { i.FundCode, i.IsinCode, i.NavDate })
            ////     .Lookup("get ")
        }
        private static Security CreateSecurityFromInstrumentType(string rbcType, string currencyIso, string isin, string name, DateTime? nextCouponDate)
        {
            var rbcCode = rbcType.Split(':')[0].Trim();

            switch (rbcCode)
            {
                case "100":
                case "102":
                case "103":
                case "120":
                case "117": // REITS
                case "118": // NON G.T. REITS
                case "411": // SICAF
                    return new Equity { CurrencyIso = currencyIso, Isin = isin, Name = name };
                case "484":
                case "485":
                    return new SubFund { CurrencyIso = currencyIso, Isin = isin, Name = name };
                case "200":
                case "201":
                case "270": // Commercial paper
                case "271": // Certificate of Deposit
                case "202": // "202 : ZERO COUPON BONDS"
                    return new Bond { CurrencyIso = currencyIso, Isin = isin, Name = name, NextCouponDate = nextCouponDate };
                case "603": // Call/Put
                    return new Derivative { CurrencyIso = currencyIso, Isin = isin, Name = name };
            }
            throw new Exception("Unrecognized code: " + rbcType);
            //100 : SHARES
            //484 : Investment Funds - UCITS- French
            //117 : G.T. REITS
            //485 : Investment Funds - UCITS- European
            //200 : STRAIGHT BONDS
            //201 : FLOATING RATE BONDS
            //270 : COMMERCIAL PAPER
            //202 : ZERO COUPON BONDS
        }

    }
}
