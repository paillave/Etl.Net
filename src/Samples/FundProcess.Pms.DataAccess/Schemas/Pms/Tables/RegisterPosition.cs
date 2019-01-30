using System;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class RegisterPosition : IBelongsToEntity
    {
        public int Id { get; set; }
        public int RegisterAccountId { get; set; }
        public RegisterAccount RegisterAccount { get; set; }
        public int ShareClassId { get; set; }
        public ShareClass ShareClass { get; set; }
        public decimal NbShares { get; set; }
        public DateTime HoldingDate { get; set; }
        public decimal? MarketValueInShareClassCcy { get; set; }
        public decimal MarketValueInSubFundCcy { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}