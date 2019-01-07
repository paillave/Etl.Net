using System;

namespace FundProcess.Pms.DataAccess.Schemas.Fee.Tables
{
    public class Fee : IBelongsToEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int? FeeDefinitionId { get; set; }
        public FeeDefinition Definition { get; set; }
        public decimal AumAssetPart { get; set; }
        public decimal AumSubFund { get; set; }
        public decimal AumSicav { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal? FeeAmountVatIncluded { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}