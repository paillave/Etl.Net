using System;
using FundProcess.Pms.Imports.StreamTypes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Extensions;
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
using FundProcess.Pms.Imports.StreamTypes.InputOutput;

namespace FundProcess.Pms.Imports.Jobs
{
    public class EfaJobs
    {
        public static void FullInitialImportProcessWithNoCtx(ISingleStream<EfaImportFilesConfigNoCtx> configStream)
        {
            FullInitialImport(configStream.Select("create config with Ctx",
                c => new EfaImportFilesConfigCtx
                {
                    InputFilesRootFolderPath = c.InputFilesRootFolderPath,
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
        public static void FullInitialImport(ISingleStream<EfaImportFilesConfigCtx> configStream)
        {
            var dbCnxStream = configStream
                .Select("get dbcnx", i => i.DbContext, true);

            var navFileStream = configStream
                .CrossApplyFolderFiles("get all Nav files", i => i.InputFilesRootFolderPath, i => i.NavFileFileNamePattern, true)
                .CrossApplyTextFile("parse nav file", new EfaNavFileDefinition());

            var posFileStream = configStream
                .CrossApplyFolderFiles("get all position files", i => i.InputFilesRootFolderPath, i => i.PositionFileFileNamePattern, true)
                .CrossApplyTextFile("parse position file", new EfaPositionFileDefinition(), i => i.Name);

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
                    CurrencyIso = i.FromFile.SubFundCurrency,
                    IsManaged = true
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

            var compositionStream = posFileStream
                .Distinct("distinct composition for a date", i => new { i.SubFundCode, i.ValuationDate })
                .Lookup("get composition sub fund", subFundStream, i => i.SubFundCode, i => i.InternalCode, (l, r) => new { FromFile = l, SubFund = r })
                .ThroughEntityFrameworkCore("save composition", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new PortfolioComposition
                {
                    Date = i.FromFile.ValuationDate,
                    PortfolioId = i.SubFund.Id
                }),
                    i => new { i.PortfolioId, i.Date, i.BelongsToEntityId },
                    (i, j) => new
                    {
                        i.FromFile,
                        Composition = j
                    });

            var composingSecuritiesStream = posFileStream
                .Distinct("distinct positions security", i => i.InstrumentCode)
                .Select("create security for composition", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(CreateSecurityForComposition(i.InstrumentCategory, i.Category1, i.Category2, i.InstrumentCode, i.InstrumentName, i.InstrumentIsin, i.InstrumentCurrency)), false, true)
                .ThroughEntityFrameworkCore("save security for composition", dbCnxStream, i => new { i.InternalCode, i.BelongsToEntityId });

            var composingSecurityStream = posFileStream
                .Lookup("lookup for composition security", composingSecuritiesStream, i => i.InstrumentCode, i => i.InternalCode, (l, r) => new { FromFile = l, Security = r })
                .Where("exclude composition security with no security", i => i.Security != null)
                .Lookup("get related composition", compositionStream,
                    i => new { i.FromFile.SubFundCode, i.FromFile.ValuationDate },
                    i => new { i.FromFile.SubFundCode, i.FromFile.ValuationDate },
                    (l, r) => new
                    {
                        l.FromFile.Quantity,
                        l.FromFile.MarketValue,
                        l.FromFile.MarketValueInInstrumentCurrency,
                        r.Composition,
                        l.Security
                    })
                .Select("create position", dbCnxStream, (i, ctx) => ctx.UpdateEntityForMultiTenancy(new Position
                {
                    SecurityId = i.Security.Id,
                    MarketValueInPortfolioCcy = i.MarketValue,
                    MarketValueInSecurityCcy = i.MarketValueInInstrumentCurrency,
                    PortfolioCompositionId = i.Composition.Id,
                    Value = i.Quantity
                }))
                .ThroughEntityFrameworkCore("save position", dbCnxStream, i => new { i.SecurityId, i.PortfolioCompositionId, i.BelongsToEntityId });
        }

        private static Security CreateSecurityForComposition(string instrumentCategory, string category1, string category2, string instrumentCode, string instrumentName, string instrumentIsin, string instrumentCurrency)
        {
            Security security = null;
            switch (instrumentCategory.ToLower())
            {
                case "tres":
                case "cpon":
                case "agde":
                case "avfi":
                case "capi":
                case "char":
                case "cmov":
                case "comp":
                case "cpcr":
                case "crne":
                case "devi":
                case "div":
                case "fcha":
                case "fis ":
                case "frai":
                case "indi":
                case "lat":
                case "matp":
                case "opcl":
                case "part":
                case "port":
                case "prod":
                case "reme":
                case "rev":
                case "rtro":
                case "sic":
                case "sr":
                case "swac":
                case "taux":
                case "ters":
                case "tisl":
                case "tota":
                case "tree":
                case "trev":
                case "trs":
                case "vfnm":
                case "zgat":
                case "zgde":
                case "ztrd":
                    security = new Cash();
                    break;
                case "opti":
                    security = new Option { Type = OptionType.European };
                    break;
                case "futu":
                    security = new Future();
                    break;
                case "swat":
                    security = new Swap();
                    break;
                case "cat":
                    security = new FxForward();
                    break;
                case "vmob":
                    security = CreateSecurityForCompositionIfVmob(category1, category2);
                    break;
                case "cfd":
                    security = new Cfd();
                    break;
                //case "abnp":
                //case "abtp":
                //case "all":
                //case "delta":
                //case "dpt":
                //case "emet":
                //case "frt":
                //case "iism":
                //case "iiso":
                //case "iisp":
                //case "opc":
                //case "swar":
                //case "swav":
                //case "tie":
                //case "trew":
                //    security = null;
                //    break;
            }
            if (security != null)
            {
                security.InternalCode = instrumentCode;
                security.CurrencyIso = instrumentCurrency;
                security.Isin = instrumentIsin;
                security.Name = instrumentName;
            }
            return security;
        }
        private static Security CreateSecurityForCompositionIfVmob(string subCategory1, string subCategory2)
        {
            switch (subCategory2.ToLower())
            {
                case "112010": //Asset_backed_securities
                case "112000": //bonds
                case "112009": //certificate_of_negociable_deposit
                case "111953": //equity_linked_notes
                case "112953": //equity_linked_notes_2
                case "119953": //equity_linked_notes_3
                case "112012": //euro_medium_term_notes
                case "112014": //medium_term_certificates
                case "113000": //money_market_instruments
                case "112099": //other_bonds
                case "112008": //reverse_convertible_notes
                case "112011": //treasury_bills
                case "112013": //treasury_bonds
                case "111690": //certificats_immobiliers
                case "141000": //immeubles
                case "111954": //obligations_liees_a_d_autres_instruments_financiers
                case "112954": //obligations_liees_a_d_autres_instruments_financiers_2
                case "112951": //obligations_liees_a_un_indice
                case "112950": //obligations_liees_a_un_panier_d_actions
                case "112955": //obligations_liees_a_un_panier_obligataire
                case "112100": //sukuk
                    return new Bond();
                case "121160": // commodities_options
                case "121130": //currency_options
                case "121150": //future_options
                case "121140": //index_options
                case "121120": //interest_rate_options
                case "121165": //issued_commodities_options
                case "121135": //issued_currency_options
                case "121155": //issued_future_options
                case "121145": //issued_index_options
                case "121125": //issued_interest_rate_options
                case "121005": //issued_options
                case "121115": //issued_options_on_transferable_securities
                case "121195": //issued_swap_options
                case "121000": //options
                case "121110": //options_on_transferable_securities
                case "121190": //swap_options
                case "114000": //warrants_and_rights
                case "121185": //options_emises_sur_risque_de_credit
                case "121180": //options_sur_risque_de_credit
                    return new Option();
                case "152319": //cfd
                case "131005": //current_accounts_at_bank
                case "999999": //current_accounts_at_bank_2
                case "122050": //guarantee_deposits_on_reverse_repurchase_agreements
                case "112007": //mezzanine_loans
                case "131013": //notification_deposits
                case "115500": //repurchase_agreements
                case "115000": //reverse_repurchase_agreements
                case "131010": //term_deposits
                    return new Cash();
                case "112952": //equity_linked_certificates_1
                case "119952": //equity_linked_certificates
                case "111600": //investment_certificates
                case "111700": //participating_certificates
                case "111400": //participating_shares
                case "111000": //shares
                case "111900": //certificats_d_investissements
                case "111952": //equity_linked_zertifikate
                    return new Stock();
                case "140": //foreign_exchange_contracts_linked_to_hedged_shares
                case "130": //forward_foreign_exchange_contracts
                    return new FxForward();
                case "161500": //tracker_funds_opc
                case "161150": //tracker_funds_opcvm
                case "111500": //undertakings_for_collective_investment
                case "111951": //certificats_lies_a_un_indice
                case "111950": //certificats_lies_a_un_panier_de_titres
                case "112990": //fonds_commun_de_creance
                case "111550": //fonds_d_investissement_fermes
                case "161200": //fonds_d_investissement_opc
                case "161100": //fonds_d_investissement_opcvm
                case "111590": //fonds_immobiliers
                case "161700": //fonds_immobiliers_opc
                case "111580": //tracker_funds
                case "111650": //tracker_funds_fermes
                    return new SubFund();
                    //case "111959": //finanzinnovationen
                    //break;
                    //case "112959": //finanzinnovationen_2
                    //break;
            }
            return null;

            // subCategory1:
            //INVESTMENTS_IN_SECURITIES = 110000  ,
            //SHORT_POSITIONS_IN_SECURITIES = 115000  ,
            //REVERSE_REPURCHASE_AGREEMENTS = 117500  ,
            //REPURCHASE_AGREEMENTS = 117550  ,
            //FINANCIAL_INSTRUMENTS = 120000  ,
            //OPTIONS = 121000  ,
            //CASH_AT_BANKS = 131000  ,
            //BANK_LIABILITIES = 131500  ,
            //REAL_ESTATE_VALUES = 141000  ,
            //PRECIOUS_METALS = 150000  ,
            //OTHER_NET_ASSETS_LIABILITIES = 152000  ,
            //Liabilities = 200000
        }
    }
}
