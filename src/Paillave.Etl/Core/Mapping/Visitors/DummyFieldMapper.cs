using System;

namespace Paillave.Etl.Core.Mapping.Visitors;

public class DummyFieldMapper : IFieldMapper
{
    public MappingSetterDefinition? MappingSetter { get; private set; } = null;
    #region boolean
    public bool ToBooleanColumn(string columnName, string trueValue, string falseValue)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            TrueValues = new[] { trueValue },
            FalseValues = new[] { falseValue },
            ColumnName = columnName,
        };
        return default;
    }

    public bool ToBooleanColumn(string columnName, string[] trueValues, string[] falseValues)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            TrueValues = trueValues,
            FalseValues = falseValues,
            ColumnName = columnName,
        };
        return default;
    }
    public bool? ToOptionalBooleanColumn(string columnName, string trueValue, string falseValue)
    {
        this.ToBooleanColumn(columnName, trueValue, falseValue);
        return default;
    }

    public bool? ToOptionalBooleanColumn(string columnName, string[] trueValues, string[] falseValues)
    {
        this.ToBooleanColumn(columnName, trueValues, falseValues);
        return default;
    }
    #endregion

    #region generic
    public T ToColumn<T>(int columnIndex)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            ColumnIndex = columnIndex,
        };
        return default;
    }
    public T ToColumn<T>(string columnName)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            ColumnName = columnName,
        };
        return default;
    }
    public T ToColumn<T>(int columnIndex, int size)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            ColumnIndex = columnIndex,
            Size = size,
        };
        return default;
    }
    #endregion

    #region string
    public string ToColumn(int columnIndex)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            ColumnIndex = columnIndex,
        };
        return default;
    }
    public string ToColumn(string columnName)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            ColumnName = columnName,
        };
        return default;
    }
    public string ToColumn(int columnIndex, int size)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            ColumnIndex = columnIndex,
            Size = size,
        };
        return default;
    }
    #endregion

    #region number
    public T ToNumberColumn<T>(int columnIndex, string decimalSeparator, string groupSeparator)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DecimalSeparator = decimalSeparator,
            GroupSeparator = groupSeparator,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public T ToNumberColumn<T>(string columnName, string decimalSeparator, string groupSeparator)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DecimalSeparator = decimalSeparator,
            GroupSeparator = groupSeparator,
            ColumnName = columnName,
        };
        return default;
    }

    public T ToNumberColumn<T>(int columnIndex, int size, string decimalSeparator, string groupSeparator)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DecimalSeparator = decimalSeparator,
            GroupSeparator = groupSeparator,
            ColumnIndex = columnIndex,
            Size = size,
        };
        return default;
    }

    public T ToNumberColumn<T>(int columnIndex, string decimalSeparator)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DecimalSeparator = decimalSeparator,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public T ToNumberColumn<T>(string columnName, string decimalSeparator)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DecimalSeparator = decimalSeparator,
            ColumnName = columnName,
        };
        return default;
    }

    public T ToNumberColumn<T>(int columnIndex, int size, string decimalSeparator)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DecimalSeparator = decimalSeparator,
            ColumnIndex = columnIndex,
            Size = size,
        };
        return default;
    }
    #endregion
    #region date
    public DateTime ToCulturedDateColumn(int columnIndex, string cultureName)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public DateTime ToCulturedDateColumn(string columnName, string cultureName)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            ColumnName = columnName,
        };
        return default;
    }

    public DateTime ToCulturedDateColumn(int columnIndex, string cultureName, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            DateFormat = dateFormat,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public DateTime ToCulturedDateColumn(string columnName, string cultureName, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            DateFormat = dateFormat,
            ColumnName = columnName,
        };
        return default;
    }

    public DateTime ToCulturedDateColumn(int columnIndex, int size, string cultureName)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            Size = size,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public DateTime ToCulturedDateColumn(int columnIndex, int size, string cultureName, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            Size = size,
            DateFormat = dateFormat,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public DateTime ToDateColumn(int columnIndex, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DateFormat = dateFormat,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public DateTime ToDateColumn(string columnName, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DateFormat = dateFormat,
            ColumnName = columnName,
        };
        return default;
    }

    public DateTime ToDateColumn(int columnIndex, int size, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DateFormat = dateFormat,
            ColumnIndex = columnIndex,
            Size = size,
        };
        return default;
    }

    public DateTime? ToOptionalCulturedDateColumn(int columnIndex, string cultureName)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public DateTime? ToOptionalCulturedDateColumn(string columnName, string cultureName)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            ColumnName = columnName,
        };
        return default;
    }

    public DateTime? ToOptionalCulturedDateColumn(int columnIndex, string cultureName, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            DateFormat = dateFormat,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public DateTime? ToOptionalCulturedDateColumn(string columnName, string cultureName, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            DateFormat = dateFormat,
            ColumnName = columnName,
        };
        return default;
    }

    public DateTime? ToOptionalCulturedDateColumn(int columnIndex, int size, string cultureName)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            Size = size,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public DateTime? ToOptionalCulturedDateColumn(int columnIndex, int size, string cultureName, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            CultureName = cultureName,
            Size = size,
            DateFormat = dateFormat,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public DateTime? ToOptionalDateColumn(int columnIndex, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DateFormat = dateFormat,
            ColumnIndex = columnIndex,
        };
        return default;
    }

    public DateTime? ToOptionalDateColumn(string columnName, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DateFormat = dateFormat,
            ColumnName = columnName,
        };
        return default;
    }

    public DateTime? ToOptionalDateColumn(int columnIndex, int size, string dateFormat)
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            DateFormat = dateFormat,
            ColumnIndex = columnIndex,
            Size = size,
        };
        return default;
    }

    public string ToSourceName()
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            ForSourceName = true
        };
        return default;
    }

    public int ToLineNumber()
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            ForLineNumber = true
        };
        return default;
    }
    public Guid ToRowGuid()
    {
        this.MappingSetter = new MappingSetterDefinition
        {
            ForRowGuid = true
        };
        return default;
    }

    public T Ignore<T>()
    {
        return default;
    }
    #endregion
}
