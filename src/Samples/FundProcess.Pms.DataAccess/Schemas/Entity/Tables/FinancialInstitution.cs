using FundProcess.Pms.DataAccess.Enums;

namespace FundProcess.Pms.DataAccess.Schemas.Entity.Tables
{
    public class FinancialInstitution : Company
    {
        public bool? Regulated { get; set; }
        public bool? CssfEquivalentSupervision { get; set; }
        public FinancialInstitutionType Type { get; set; }
    }
}