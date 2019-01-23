using System;
using System.Collections.ObjectModel;
using FundProcess.Pms.DataAccess.Enums;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class Security : IBelongsToEntity
    {
        public int Id { get; set; }
        public string InternalCode { get; set; }
        // public SecurityType Type { get; set; }
        public string Isin { get; set; }
        public string Name { get; set; }
        public int? BenchmarkId { get; set; }
        public Security Benchmark { get; set; }
        public string CurrencyIso { get; set; }
        public string CountryCode { get; set; }
        public FrequencyType PricingFrequency { get; set; }
        public string ClassificationStrategy { get; set; }
        public int? MarketPlaceId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Collection<SecurityGroupItem> Groups { get; set; }
        public int? BelongsToEntityId { get; set; }
    }
}