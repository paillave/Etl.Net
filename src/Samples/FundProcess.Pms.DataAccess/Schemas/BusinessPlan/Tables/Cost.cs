using System;
using FundProcess.Pms.DataAccess.Enums;

namespace FundProcess.Pms.DataAccess.Schemas.BusinessPlan.Tables
{
    public class Cost
    {
        public int Id { get; set; }
        public string InternalCode { get; set; }
        public int Category { get; set; }
        public string Name { get; set; }
        public double Amount { get; set; }
        public FrequencyType Frequency { get; set; }
        public DateTime FirstPaymentDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ManCoId { get; set; }
    }
}