namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class SecurityGroupItem : IBelongsToEntity
    {
        public int SecurityId { get; set; }
        public Security Security { get; set; }
        public int SecurityGroupId { get; set; }
        public SecurityGroup SecurityGroup { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}