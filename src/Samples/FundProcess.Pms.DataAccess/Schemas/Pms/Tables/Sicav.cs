using FundProcess.Pms.DataAccess.Enums;
using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class Sicav : IBelongsToEntity
    {
        public int Id { get; set; }
        public string SicavName { get; set; }
        public int? ProspectusId { get; set; }
        public SicavStructureType? LegalStructure { get; set; }
        public int? BelongsToEntityId { get; set; }
        public ManCo ManCo { get; set; }
    }
}