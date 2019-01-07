namespace FundProcess.Pms.DataAccess.Schemas.BusinessPlan.Tables
{
    public class CostForecast
    {
        public int ScenarioId { get; set; }
        public int CategoryId { get; set; }
        public double IndexationRate { get; set; }
    }
}