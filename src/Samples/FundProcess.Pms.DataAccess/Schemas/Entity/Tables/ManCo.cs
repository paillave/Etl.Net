namespace FundProcess.Pms.DataAccess.Schemas.Entity.Tables
{
    public class ManCo : FinancialInstitution
    {
        public bool CollectiveManagement { get; set; }
        public bool DiscretionaryManagement { get; set; }
        public bool Aifm { get; set; }
    }
}