using FundProcess.Pms.DataAccess.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FundProcess.Pms.DataAccess.ValueConverters
{
    public class FinancialInstitutionTypeValueConverter : EnumToNumberConverter<FinancialInstitutionType, int>
    {

    }
}