namespace FundProcess.Pms.DataAccess.Schemas.Entity.Tables
{
    public class InvestorRelationshipItem : IBelongsToEntity
    {
        public int InvestorRelationshipId { get; set; }
        public InvestorRelationship InvestorRelationship { get; set; }
        public int InvestorId { get; set; }
        public Investor Investor { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}