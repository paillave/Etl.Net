using System;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class PortfolioComposition : IBelongsToEntity
    {
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        public Security Portfolio { get; set; }
        public DateTime? Date { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}