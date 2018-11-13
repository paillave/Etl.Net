using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Paillave.Etl.Core.Mapping
{
    public class MappingSetterDefinition
    {
        public string ColumnName { get; internal set; } = null;
        public int? ColumnIndex { get; internal set; } = null;
        public string DateFormat { get; internal set; } = null;
        public PropertyInfo TargetPropertyInfo { get; internal set; }
        public string DecimalSeparator { get; internal set; } = null;
        public string CultureName { get; internal set; } = null;
        public int? Size { get; internal set; } = null;

        public CultureInfo CreateCultureInfo()
        {
            if (new[] {
                typeof(DateTime),
                typeof(DateTime?),
            }.Contains(TargetPropertyInfo.PropertyType))
            {
                if (DateFormat == null && CultureName == null) return null;
                CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureName ?? "en-GB");
                if (this.DateFormat == null) return ci;
                ci.DateTimeFormat.FullDateTimePattern = DateFormat;
                ci.DateTimeFormat.LongDatePattern = DateFormat;
                ci.DateTimeFormat.ShortDatePattern = DateFormat;

                ci.DateTimeFormat.FullDateTimePattern = DateFormat;
                ci.DateTimeFormat.LongDatePattern = DateFormat;
                ci.DateTimeFormat.ShortDatePattern = DateFormat;
                return ci;
            }
            if (new[] {
                typeof(System.Byte),
                typeof(System.SByte),
                typeof(System.Decimal),
                typeof(System.Double),
                typeof(System.Single),
                typeof(System.Int32),
                typeof(System.UInt32),
                typeof(System.Int64),
                typeof(System.UInt64),
                typeof(System.Int16),
                typeof(System.UInt16),
                typeof(System.Byte?),
                typeof(System.SByte?),
                typeof(System.Decimal?),
                typeof(System.Double?),
                typeof(System.Single?),
                typeof(System.Int32?),
                typeof(System.UInt32?),
                typeof(System.Int64?),
                typeof(System.UInt64?),
                typeof(System.Int16?),
                typeof(System.UInt16?),
            }.Contains(TargetPropertyInfo.PropertyType))
            {
                if (DecimalSeparator != null)
                {
                    CultureInfo ci = CultureInfo.CreateSpecificCulture(string.Empty);
                    ci.NumberFormat.NumberDecimalSeparator = DecimalSeparator;
                    ci.NumberFormat.CurrencyDecimalSeparator = DecimalSeparator;
                    ci.NumberFormat.PercentDecimalSeparator = DecimalSeparator;
                    return ci;
                }
            }
            if (CultureName != null) return CultureInfo.CreateSpecificCulture(CultureName);
            return null;
        }
        //public Type PropertyType { get; set; } = null;
    }
}
