using System;
using FundProcess.Pms.DataAccess.Enums;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class ShareClass : Security
    {
        public int SubFundId { get; set; }
        public SubFund SubFund { get; set; }
        public bool IsPrimary { get; set; }
        public DistributionType? DistributionType { get; set; }
        public InvestorType? InvestorType { get; set; }
        public DateTime? InceptionDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public decimal? MinimumInvestment { get; set; }
        public decimal? EntryFee { get; set; }
        public decimal? ExitFee { get; set; }
        public decimal? ManagementFee { get; set; }
        public decimal? PerformanceFee { get; set; }
        public FrequencyType? DividendPeriodicity { get; set; }
        public bool IsOpenForInvestment { get; set; }
    }
}