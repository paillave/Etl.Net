using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class RegisterAccount : IBelongsToEntity
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string SortName { get; set; }
        public string DealerTaCode { get; set; }
        public int? ShareHolderId { get; set; }
        public Investor ShareHolder { get; set; }
        public int? DistributorId { get; set; }
        public FinancialInstitution Distributor { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}