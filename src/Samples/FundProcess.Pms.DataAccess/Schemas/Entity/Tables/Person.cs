using System.Collections.ObjectModel;

namespace FundProcess.Pms.DataAccess.Schemas.Entity.Tables
{
    public class Person : EntityBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public string IdCardNumber { get; set; }
        public string PassportNumber { get; set; }
        public Collection<PersonToCompany> Companies { get; set; }
    }
}