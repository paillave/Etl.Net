using System;

namespace FundProcess.Pms.Imports.StreamTypes
{
    public class RbcPositionFile
    {
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public string SubfundCcy { get; set; }
        public DateTime NavDate { get; set; }
        public string AccNumber { get; set; }
        public string InternalNumber { get; set; }
        public string IsinCode { get; set; }
        public string InstrumentName { get; set; }
        public string Currency { get; set; }
        public decimal Quantity { get; set; }
        public decimal MarketValueInFdCcy { get; set; }
        public decimal MarketValueInSecCcy { get; set; }
        public decimal Tna { get; set; }
        public decimal PercNav { get; set; }
        public decimal PercPortfolio { get; set; }
        public string IdBloomberg { get; set; }
        public DateTime? NextCouponDate { get; set; }
        public decimal ValuationPrice { get; set; }
        public decimal MarketPrice { get; set; }
        public DateTime? LastCouponDate { get; set; }
        public decimal AccruedIntFdCcy { get; set; }
        public decimal AccruedInt { get; set; }
        public int? NumberOfAccruedDays { get; set; }
        public string InvestmentType { get; set; }
        public string GeographicalSector { get; set; }
        public string EconomicSectorCode { get; set; }
        public string EconomicSectorLabel { get; set; }
    }
}