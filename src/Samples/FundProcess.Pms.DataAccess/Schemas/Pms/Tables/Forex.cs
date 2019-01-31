using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class Forex : IBelongsToEntity
    {
        public string CurrencyFromIso { get; set; }
        public string CurrencyToIso { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}
