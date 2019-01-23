using System;

namespace FundProcess.Pms.Imports.StreamTypes
{
    public class EfaNavFile
    {
        public string ShareCode { get; set; }//Share_code
        public string SubFundCode { get; set; }//Sub-fund_code
        public string IsinCode { get; set; }//Isin_code
        public string ShareCurrency { get; set; }//CCY_NAV_share
        public DateTime NavDate { get; set; }//Valuation_date
        public decimal NavPerShare { get; set; }//NAV_share
        public decimal TotalNetAsset { get; set; }//Total_Net_Assets
        public decimal NetAssetShareType { get; set; }//Net_assets_share_type
    }
}