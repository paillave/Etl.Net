using System.Collections.Generic;
using System.Globalization;
using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;

namespace FundProcess.Pms.DataAccess.Schemas.UserAccounts.Tables
{
    public class User : Person
    {
        public CultureInfo Culture { get; set; }
        public ICollection<UserEntityRole> Roles { get; set; }
        public ICollection<UserLogin> Logins { get; set; }
    }
}