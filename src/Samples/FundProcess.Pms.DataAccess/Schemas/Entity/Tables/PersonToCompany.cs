namespace FundProcess.Pms.DataAccess.Schemas.Entity.Tables
{
    public class PersonToCompany : IBelongsToEntity
    {
        public Person Person { get; set; }
        public int PersonId { get; set; }
        public Company Company { get; set; }
        public int CompanyId { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}