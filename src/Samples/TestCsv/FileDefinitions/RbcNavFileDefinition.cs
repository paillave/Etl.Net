using System;
using TestCsv.StreamTypes;
using Paillave.Etl.TextFile.Core;

namespace TestCsv.FileDefinitions
{
    public class RbcNavFileDefinition : FlatFileDefinition<RbcNavFile>
    {
        public RbcNavFileDefinition()
        {
            this.IsColumnSeparated(',');
            this.WithMap(i => new RbcNavFile
            {
                AmountRedemption = i.ToNumberColumn<decimal>("AMOUNT REDEMPTION", "."),
                AmountSubscription = i.ToNumberColumn<decimal>("AMOUNT SUBSCRIPTION", "."),
                Currency = i.ToColumn<string>("CURRENCY"),
                FundCode = i.ToColumn<string>("FUND CODE"),
                FundName = i.ToColumn<string>("FUND NAME"),
                FundTotalNetAsset = i.ToNumberColumn<decimal>("FUND TOTAL NET ASSET", "."),
                IsinCode = i.ToColumn<string>("ISIN CODE"),
                NavDate = i.ToDateColumn<DateTime>("NAV DATE", "yyyyMMdd"),
                NavPerShare = i.ToNumberColumn<decimal>("NAV PER SHARE", "."),
                PreviousNav = i.ToNumberColumn<decimal>("PREVIOUS NAV", "."),
                QuantityRedemption = i.ToNumberColumn<decimal>("QUANTITY REDEMPTION", "."),
                QuantitySubscription = i.ToNumberColumn<decimal>("QUANTITY SUBSCRIPTION", "."),
                Tis = i.ToNumberColumn<decimal>("TIS", "."),
                TotalNetAsset = i.ToNumberColumn<decimal>("TOTAL NET ASSET", "."),
                TotalTisAmount = i.ToNumberColumn<decimal>("TOTAL TIS AMOUNT", "."),
            });
        }
    }
}