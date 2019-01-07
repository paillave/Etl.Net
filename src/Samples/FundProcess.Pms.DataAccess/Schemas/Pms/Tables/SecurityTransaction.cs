using System;
using FundProcess.Pms.DataAccess.Enums;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class SecurityTransaction : IBelongsToEntity
    {
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        public Security Portfolio { get; set; }
        public int SecurityId { get; set; }
        public Security Security { get; set; }
        public string StatusCode { get; set; }
        public SecurityTransactionType Type { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime NavDate { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal GrossAmountInSecurityCcy { get; set; }
        public decimal NetAmountInSecurityCcy { get; set; }
        public decimal NetAmountInFundCcy { get; set; }
        public decimal PriceInSecurityCcy { get; set; }
        public decimal PriceInFundCcy { get; set; }
        public string DealDescription { get; set; }
        public decimal TotalGainLoss { get; set; }
        public decimal FeesInSecurityCcy { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}