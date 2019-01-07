using System;
using FundProcess.Pms.DataAccess.Enums;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class HistoricalValue : IBelongsToEntity
    {
        public int SecurityId { get; set; }
        public Security Security { get; set; }
        public HistoricalValueType Type { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}