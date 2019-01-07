using System.Collections.Generic;

namespace FundProcess.Pms.DataAccess.Schemas.Entity.Tables
{
    public class EntityBase : IBelongsToEntity
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string ConnectionString { get; set; }
        public int? EntityGroupId { get; set; }
        public EntityGroup Group { get; set; }
        public bool IsActive { get; set; }
        public int? BelongsToEntityId { get; set; }
        public int? BelongsToEntityGroupId { get; set; }
    }
}