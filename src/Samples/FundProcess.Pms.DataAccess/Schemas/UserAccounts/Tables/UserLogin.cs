namespace FundProcess.Pms.DataAccess.Schemas.UserAccounts.Tables
{
    public class UserLogin : IBelongsToEntity
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string IdentityProvider { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public bool IsActive { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}