using System;
using FundProcess.Pms.DataAccess.Enums;
using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class Portfolio : Security
    {
        public int? FundAdminId { get; set; }
        public FinancialInstitution FundAdmin { get; set; }
        public int? CustodianId { get; set; }
        public FinancialInstitution Custodian { get; set; }
        public FrequencyType? NavFrequency { get; set; }
        public bool IsManaged { get; set; }
    }
}