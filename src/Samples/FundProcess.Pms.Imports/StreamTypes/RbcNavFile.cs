using System;

namespace FundProcess.Pms.Imports.StreamTypes
{
    public class RbcNavFile
    {
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public string IsinCode { get; set; }
        public string Currency { get; set; }
        public string FundCurrency { get; set; }
        public DateTime NavDate { get; set; }
        public decimal NavPerShare { get; set; }
        //public decimal? PreviousNav { get; set; }
        public decimal TotalNetAsset { get; set; }
        public decimal FundTotalNetAsset { get; set; }
        //public decimal? Tis { get; set; }
        //public decimal? TotalTisAmount { get; set; }
        public decimal? AmountRedemption { get; set; }
        public decimal? AmountSubscription { get; set; }
        //public decimal? QuantityRedemption { get; set; }
        //public decimal? QuantitySubscription { get; set; }
    }
}