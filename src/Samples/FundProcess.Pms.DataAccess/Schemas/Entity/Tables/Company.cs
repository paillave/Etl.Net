using System.Collections.ObjectModel;

namespace FundProcess.Pms.DataAccess.Schemas.Entity.Tables
{
    public class Company : EntityBase
    {
        public string Name { get; set; }
        public int? ContactId { get; set; }
        public Person Contact { get; set; }
        public decimal? VAT { get; set; }
        public string Url { get; set; }
        public string RegistrationNumber { get; set; }
        public Collection<PersonToCompany> People { get; set; }
    }
}