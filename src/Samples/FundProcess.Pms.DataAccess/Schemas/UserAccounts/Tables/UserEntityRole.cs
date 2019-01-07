using System.Collections.Generic;
using FundProcess.Pms.DataAccess.Enums;
using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;

namespace FundProcess.Pms.DataAccess.Schemas.UserAccounts.Tables
{
    public class UserEntityRole : IBelongsToEntity
    {
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public ApplicationRole ApplicationRole { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}