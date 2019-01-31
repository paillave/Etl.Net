using FundProcess.Pms.DataAccess.Enums;
using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class Option : Derivative
    {
        public decimal? StrikePrice { get; set; }
        public OptionType Type { get; set; }
    }
}