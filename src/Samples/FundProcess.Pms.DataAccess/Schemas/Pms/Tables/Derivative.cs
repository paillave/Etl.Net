using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class Derivative : Security
    {
        public bool? IsOtc { get; set; }
        public int? CounterpartyId { get; set; }
        public FinancialInstitution Counterparty { get; set; }
        public decimal? ContractSize { get; set; }
        public decimal? StrikePrice { get; set; }
    }
}