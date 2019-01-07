using System.Collections.ObjectModel;
using FundProcess.Pms.DataAccess.Enums;

namespace FundProcess.Pms.DataAccess.Schemas.Entity.Tables
{
    public class Investor : IBelongsToEntity
    {
        public int Id { get; set; }
        public int EntityId { get; set; }
        public EntityBase Entity { get; set; }
        public InvestorType Type { get; set; }
        public int? IntermediaryId { get; set; }
        public EntityBase Intermediary { get; set; }
        public int? InternalResponsibleId { get; set; }
        public Person InternalResponsible { get; set; }
        public int? BelongsToEntityId { get; set; }
        public Collection<InvestorRelationshipItem> Relationships { get; set; }
    }
}