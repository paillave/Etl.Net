using System;

namespace FundProcess.Pms.DataAccess.Schemas.BusinessPlan.Tables
{
    public class StrategyForecastThreshold
    {
        public int Id { get; set; }
        public int StrategyFortcastId { get; set; }
        public DateTime TargetDate { get; set; }
        public double StrategyAum { get; set; }
    }
}