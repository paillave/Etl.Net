using FundProcess.Pms.DataAccess.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FundProcess.Pms.DataAccess.ValueConverters
{
    public class HistoricalValueTypeValueConverter : EnumToStringConverter<HistoricalValueType>
    {

    }
}