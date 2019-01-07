namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class Position : IBelongsToEntity
    {
        public int Id { get; set; }
        public int PortfolioCompositionId { get; set; }
        public PortfolioComposition PortfolioComposition { get; set; }
        public int SecurityId { get; set; }
        public Security Security { get; set; }
        public decimal Value { get; set; }
        public decimal? Weight { get; set; }
        public decimal? MarketValueInSecurityCcy { get; set; }
        public decimal MarketValueInPortfolioCcy { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}