using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;

namespace EFCore.BulkExtensions
{
    public class BulkConfig
    {
        public bool SetOutputIdentity { get; set; }
        public int BatchSize { get; set; } = 2000;
        public int? NotifyAfter { get; set; }
        public int? BulkCopyTimeout { get; set; }
        public bool EnableStreaming { get; set; }
        public bool UseTempDB { get; set; } = false;
        public bool TrackingEntities { get; set; }
        public bool WithHoldlock { get; set; } = true;
        public List<string> UpdateByProperties { get; set; }
        public SqlBulkCopyOptions SqlBulkCopyOptions { get; set; }
    }
}