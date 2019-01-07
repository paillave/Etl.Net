using System.Collections.ObjectModel;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class SecurityGroup : IBelongsToEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}