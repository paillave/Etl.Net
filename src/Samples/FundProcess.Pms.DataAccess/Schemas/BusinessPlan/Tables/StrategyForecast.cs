namespace FundProcess.Pms.DataAccess.Schemas.BusinessPlan.Tables
{
    public class StrategyForecast
    {
        public int Id { get; set; }
        public int ScenarioId { get; set; }
        public int StrategyId { get; set; }
        public double IncreaseAnnualRate { get; set; }
    }
}