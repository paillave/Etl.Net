using System;
using Paillave.Etl.TextFile.Core;
using TestCsv.StreamTypes;

namespace TestCsv.FileDefinitions
{
    public class RbcPositionFileDefinition : FlatFileDefinition<RbcPositionFile>
    {
        public RbcPositionFileDefinition()
        {
            this.IsColumnSeparated(',');
            this.WithMap(i => new RbcPositionFile
            {
                FundCode = i.ToColumn<string>("FUND CODE"),
                FundName = i.ToColumn<string>("FUND NAME"),
                SubfundCcy = i.ToColumn<string>("SUBFUND CCY"),
                NavDate = i.ToDateColumn<DateTime>("NAV DATE", "yyyyMMdd"),
                AccNumber = i.ToColumn<string>("ACC NUMBER"),
                InternalNumber = i.ToColumn<string>("INTERNAL NUMBER"),
                IsinCode = i.ToColumn<string>("ISIN CODE"),
                InstrumentName = i.ToColumn<string>("INSTRUMENT NAME"),
                Currency = i.ToColumn<string>("CURRENCY"),
                Quantity = i.ToNumberColumn<decimal>("QUANTITY", "."),
                MarketValueInFdCcy = i.ToNumberColumn<decimal>("MARKET VALUE IN FD CCY", "."),
                MarketValueInSecCcy = i.ToNumberColumn<decimal>("MARKET VALUE IN SEC CCY", "."),
                Tna = i.ToNumberColumn<decimal>("TNA", "."),
                PercNav = i.ToNumberColumn<decimal>("% NAV", "."),
                PercPortfolio = i.ToNumberColumn<decimal>("%PORTFOLIO", "."),
                IdBloomberg = i.ToColumn<string>("ID BLOOMBERG"),
                NextCouponDate = i.ToDateColumn<DateTime>("NEXT COUPON DATE", "yyyyMMdd"),
                ValuationPrice = i.ToNumberColumn<decimal>("VALUATION PRICE", "."),
                MarketPrice = i.ToNumberColumn<decimal>("MARKET PRICE", "."),
                LastCouponDate = i.ToDateColumn<DateTime>("LAST COUPON DATE", "yyyyMMdd"),
                AccruedIntFdCcy = i.ToNumberColumn<decimal>("ACCRUED INT FD CCY", "."),
                AccruedInt = i.ToNumberColumn<decimal>("ACCRUED INT.", "."),
                NumberOfAccruedDays = i.ToColumn<int>("NUMBER OF ACCRUED DAYS"),
                InvestmentType = i.ToColumn<string>("INVESTMENT TYPE"),
                GeographicalSector = i.ToColumn<string>("GEOGRAPHICAL SECTOR"),
                EconomicSectorCode = i.ToColumn<string>("ECONOMIC SECTOR CODE"),
                EconomicSectorLabel = i.ToColumn<string>("ECONOMIC SECTOR LABEL"),
            });
        }
    }
}