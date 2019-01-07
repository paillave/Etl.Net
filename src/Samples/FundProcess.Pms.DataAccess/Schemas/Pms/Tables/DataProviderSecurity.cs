using FundProcess.Pms.DataAccess.Enums;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class DataProviderSecurity : IBelongsToEntity
    {
        public int SecurityId { get; set; }
        public string Code { get; set; }
        public DataProvider DataProvider { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}