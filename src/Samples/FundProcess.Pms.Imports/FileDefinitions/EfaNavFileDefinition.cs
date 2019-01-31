using System;
using FundProcess.Pms.Imports.StreamTypes;
using FundProcess.Pms.Imports.StreamTypes.InputOutput;
using Paillave.Etl.TextFile.Core;

namespace FundProcess.Pms.Imports.FileDefinitions
{
    public class EfaNavFileDefinition : FlatFileDefinition<EfaNavFile>
    {
        public EfaNavFileDefinition()
        {
            this.IsColumnSeparated(',');
            this.WithMap(i => new EfaNavFile
            {
                ShareCode = i.ToColumn<string>("Share_code"),
                SubFundCode = i.ToColumn<string>("Sub-fund_code"),
                IsinCode = i.ToColumn<string>("Isin_code"),
                ShareCurrency = i.ToColumn<string>("CCY_NAV_share"),
                NavDate = i.ToDateColumn<DateTime>("Valuation_date", "dd/MM/yyyy"),
                NavPerShare = i.ToDateColumn<decimal>("NAV_share", "."),
                TotalNetAsset = i.ToDateColumn<decimal>("Total_Net_Assets", "."),
                NetAssetShareType = i.ToDateColumn<decimal>("Net_assets_share_type", "."),
            });
        }
    }
}