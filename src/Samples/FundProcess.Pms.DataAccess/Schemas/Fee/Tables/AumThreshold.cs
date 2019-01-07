namespace FundProcess.Pms.DataAccess.Schemas.Fee.Tables
{
    public class AumThreshold : IBelongsToEntity
    {
        public int Id { get; set; }
        public int FeeId { get; set; }
        public Tables.Fee Fee { get; set; }
        public decimal AumFromIncluded { get; set; }
        public decimal AumToExcluded { get; set; }
        public decimal? AnnualRate { get; set; }
        // public int FeeDefinitionId { get; set; } // seem to be a useless denormalization of Fee.Definition
        public int? BelongsToEntityId { get; set; }
    }
}