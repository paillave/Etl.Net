using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;
using System;
using System.Linq.Expressions;

namespace FundProcess.Pms.DataAccess.ValueConverters
{
    public class CultureInfoValueConverter : ValueConverter<CultureInfo, string>
    {
        public CultureInfoValueConverter() : base(i => i.Name, i => new CultureInfo(i)) { }
    }
}