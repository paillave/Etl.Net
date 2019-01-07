using System.Collections.ObjectModel;

namespace FundProcess.Pms.DataAccess.Schemas.Entity.Tables
{
    public class InvestorRelationship : IBelongsToEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Collection<InvestorRelationshipItem> Investors { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}