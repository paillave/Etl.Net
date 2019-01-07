using System;
using FundProcess.Pms.DataAccess.Enums;
using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class SubFund : Security
    {
        public int? SicavId { get; set; }
        public Sicav Sicav { get; set; }
        public int? FundAdminId { get; set; }
        public FinancialInstitution FundAdmin { get; set; }
        public int? CustodianId { get; set; }
        public FinancialInstitution Custodian { get; set; }
        public int? TransferAgentId { get; set; }
        public FinancialInstitution TransferAgent { get; set; }
        public string Url { get; set; }
        public string DomicileIso { get; set; }
        public int? SubscriptionContactId { get; set; }
        public Person SubscriptionContact { get; set; }
        public decimal? RecommendedTimeHorizon { get; set; }
        public int? SettlementNbDays { get; set; }
        public FrequencyType? NavFrequency { get; set; }
        public bool? IsLiquidated { get; set; }
        public DateTime? LiquidationDate { get; set; }
        public InvestmentProcessType? InvestmentProcess { get; set; }
        public bool? ShortExposure { get; set; }
        public bool? Leverage { get; set; }
        public bool? ClosedEnded { get; set; }
    }
}