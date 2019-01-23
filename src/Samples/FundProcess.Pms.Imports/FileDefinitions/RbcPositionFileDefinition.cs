using System;
using FundProcess.Pms.Imports.StreamTypes;
using Paillave.Etl.TextFile.Core;

namespace FundProcess.Pms.Imports.FileDefinitions
{
    public class RbcPositionFileDefinition : FlatFileDefinition<RbcPositionFile>
    {
        public RbcPositionFileDefinition()
        {
            this.IsColumnSeparated(',');
            this.WithMap(i => new RbcPositionFile
            {
                FundCode = i.ToColumn<string>("FUND CODE"),
                NavDate = i.ToDateColumn<DateTime>("NAV DATE", "yyyyMMdd"),
                InternalNumber = i.ToColumn<string>("INTERNAL NUMBER"),
                IsinCode = i.ToColumn<string>("ISIN CODE"),
                InstrumentName = i.ToColumn<string>("INSTRUMENT NAME"),
                Currency = i.ToColumn<string>("CURRENCY"),
                Quantity = i.ToNumberColumn<decimal>("QUANTITY", "."),
                MarketValueInFdCcy = i.ToNumberColumn<decimal>("MARKET VALUE IN FD CCY", "."),
                MarketValueInSecCcy = i.ToNumberColumn<decimal>("MARKET VALUE IN SEC CCY", "."),
                PercNav = i.ToNumberColumn<decimal>("% NAV", "."),
                NextCouponDate = i.ToDateColumn<DateTime?>("NEXT COUPON DATE", "yyyyMMdd"),
                InvestmentType = i.ToColumn<string>("INVESTMENT TYPE"),
            });
        }
    }
}
