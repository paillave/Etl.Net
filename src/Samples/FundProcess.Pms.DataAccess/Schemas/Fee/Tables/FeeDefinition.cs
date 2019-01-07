using System;
using FundProcess.Pms.DataAccess.Enums;
using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;
using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;

namespace FundProcess.Pms.DataAccess.Schemas.Fee.Tables
{
    public class FeeDefinition : IBelongsToEntity
    {
        public int Id { get; set; }
        public int? PortfolioId { get; set; }
        public Security Portfolio { get; set; }
        public int? ShareClassId { get; set; }
        public ShareClass ShareClass { get; set; }
        public int? SicavId { get; set; }
        public Sicav Sicav { get; set; }
        public decimal? AnnualRate { get; set; }
        public int? RegisterAccountId { get; set; }
        public RegisterAccount RegisterAccount { get; set; }
        public int ThirdPartyId { get; set; }
        public EntityBase ThirdParty { get; set; }
        public AssetPart AssetPart { get; set; }
        public int? ManCoSecuritiesId { get; set; }
        public SecurityGroup ManCoSecurities { get; set; }
        public bool IncludeCashInAum { get; set; }
        public DateTime? ValidityFrom { get; set; }
        public DateTime? ValidityTo { get; set; }
        public FeeType FeeType { get; set; }
        public bool IsVatApplicable { get; set; }
        public bool? VatValue { get; set; }
        public FrequencyType PaymentFrequency { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}