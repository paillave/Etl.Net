using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.StreamTypes.InputOutput
{
    public class RbcAccountPositionFile
    {
        public string Isin { get; set; }
        public string AccountNumber { get; set; }
        public DateTime HoldingDate { get; set; }
        public decimal SharesBalance { get; set; } //qty
        public decimal Assets { get; set; } //amnt
    }
}
