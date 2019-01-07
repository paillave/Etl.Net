using System;
using FundProcess.Pms.DataAccess.Enums;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class Bond : Security
    {
        public string CouponType { get; set; }
        public decimal? CouponRate { get; set; }
        public decimal? FaceValue { get; set; }
        public decimal? Notional { get; set; }
        public DateTime? MaturityDate { get; set; }
        public bool? IsPerpetual { get; set; }
        public DateTime? FirstPaymentDate { get; set; }
        public DateTime? NextCouponDate { get; set; }
        public FrequencyType? PaymentFrequency { get; set; }
        public decimal? IssueAmount { get; set; }
    }
}