using System;

namespace Paillave.Etl.SqlServer.Core
{
    public class DummyFieldMapper : ISqlResultMapper
    {
        public string ColumnName { get; set; }

        public bool ToBooleanColumn(string columnName)
        {
            this.ColumnName = columnName;
            return default;
        }

        public bool ToBooleanColumn()
        {
            return default;
        }

        public T ToColumn<T>(string columnName)
        {
            this.ColumnName = columnName;
            return default;
        }

        public string ToColumn(string columnName)
        {
            this.ColumnName = columnName;
            return default;
        }

        public T ToColumn<T>()
        {
            return default;
        }

        public string ToColumn()
        {
            return default;
        }

        public DateTime ToDateColumn(string columnName)
        {
            this.ColumnName = columnName;
            return default;
        }

        public DateTime ToDateColumn()
        {
            return default;
        }

        public T ToNumberColumn<T>(string columnName)
        {
            this.ColumnName = columnName;
            return default;
        }

        public T ToNumberColumn<T>()
        {
            return default;
        }

        public bool? ToOptionalBooleanColumn(string columnName)
        {
            this.ColumnName = columnName;
            return default;
        }

        public bool? ToOptionalBooleanColumn()
        {
            return default;
        }

        public DateTime? ToOptionalDateColumn(string columnName)
        {
            this.ColumnName = columnName;
            return default;
        }

        public DateTime? ToOptionalDateColumn()
        {
            return default;
        }
        // #region boolean
        // public bool ToBooleanColumn(string columnName, string trueValue, string falseValue)
        // {
        //     this.MappingSetter.TrueValues = new[] { trueValue };
        //     this.MappingSetter.FalseValues = new[] { falseValue };
        //     this.MappingSetter.ColumnName = columnName;
        //     return default;
        // }

        // public bool ToBooleanColumn(string columnName, string[] trueValues, string[] falseValues)
        // {
        //     this.MappingSetter.TrueValues = trueValues;
        //     this.MappingSetter.FalseValues = falseValues;
        //     this.MappingSetter.ColumnName = columnName;
        //     return default;
        // }
        // public bool? ToOptionalBooleanColumn(string columnName, string trueValue, string falseValue)
        // {
        //     this.ToBooleanColumn(columnName, trueValue, falseValue);
        //     return default;
        // }

        // public bool? ToOptionalBooleanColumn(string columnName, string[] trueValues, string[] falseValues)
        // {
        //     this.ToBooleanColumn(columnName, trueValues, falseValues);
        //     return default;
        // }
        // #endregion

        // #region generic
        // public T ToColumn<T>(int columnIndex)
        // {
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     return default;
        // }
        // public T ToColumn<T>(string columnName)
        // {
        //     this.MappingSetter.ColumnName = columnName;
        //     return default;
        // }
        // public T ToColumn<T>(int columnIndex, int size)
        // {
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     this.MappingSetter.Size = size;
        //     return default;
        // }
        // #endregion

        // #region string
        // public string ToColumn(int columnIndex)
        // {
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     return default;
        // }
        // public string ToColumn(string columnName)
        // {
        //     this.MappingSetter.ColumnName = columnName;
        //     return default;
        // }
        // public string ToColumn(int columnIndex, int size)
        // {
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     this.MappingSetter.Size = size;
        //     return default;
        // }
        // #endregion

        // #region number
        // public T ToNumberColumn<T>(int columnIndex, string decimalSeparator, string groupSeparator)
        // {
        //     this.MappingSetter.DecimalSeparator = decimalSeparator;
        //     this.MappingSetter.GroupSeparator = groupSeparator;
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     return default;
        // }

        // public T ToNumberColumn<T>(string columnName, string decimalSeparator, string groupSeparator)
        // {
        //     this.MappingSetter.DecimalSeparator = decimalSeparator;
        //     this.MappingSetter.GroupSeparator = groupSeparator;
        //     this.MappingSetter.ColumnName = columnName;
        //     return default;
        // }

        // public T ToNumberColumn<T>(int columnIndex, int size, string decimalSeparator, string groupSeparator)
        // {
        //     this.MappingSetter.DecimalSeparator = decimalSeparator;
        //     this.MappingSetter.GroupSeparator = groupSeparator;
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     return default;
        // }

        // public T ToNumberColumn<T>(int columnIndex, string decimalSeparator)
        // {
        //     this.MappingSetter.DecimalSeparator = decimalSeparator;
        //     this.MappingSetter.GroupSeparator = null;
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     return default;
        // }

        // public T ToNumberColumn<T>(string columnName, string decimalSeparator)
        // {
        //     this.MappingSetter.DecimalSeparator = decimalSeparator;
        //     this.MappingSetter.GroupSeparator = null;
        //     this.MappingSetter.ColumnName = columnName;
        //     return default;
        // }

        // public T ToNumberColumn<T>(int columnIndex, int size, string decimalSeparator)
        // {
        //     this.MappingSetter.DecimalSeparator = decimalSeparator;
        //     this.MappingSetter.GroupSeparator = null;
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     return default;
        // }
        // #endregion
        // #region date
        // public DateTime ToCulturedDateColumn(int columnIndex, string cultureName)
        // {
        //     this.MappingSetter.CultureName = cultureName;
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     return default;
        // }

        // public DateTime ToCulturedDateColumn(string columnName, string cultureName)
        // {
        //     this.MappingSetter.CultureName = cultureName;
        //     this.MappingSetter.ColumnName = columnName;
        //     return default;
        // }

        // public DateTime ToCulturedDateColumn(int columnIndex, string cultureName, string dateFormat)
        // {
        //     this.MappingSetter.CultureName = cultureName;
        //     this.MappingSetter.DateFormat = dateFormat;
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     return default;
        // }

        // public DateTime ToCulturedDateColumn(string columnName, string cultureName, string dateFormat)
        // {
        //     this.MappingSetter.CultureName = cultureName;
        //     this.MappingSetter.DateFormat = dateFormat;
        //     this.MappingSetter.ColumnName = columnName;
        //     return default;
        // }

        // public DateTime ToCulturedDateColumn(int columnIndex, int size, string cultureName)
        // {
        //     this.MappingSetter.CultureName = cultureName;
        //     this.MappingSetter.Size = size;
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     return default;
        // }

        // public DateTime ToCulturedDateColumn(int columnIndex, int size, string cultureName, string dateFormat)
        // {
        //     this.MappingSetter.CultureName = cultureName;
        //     this.MappingSetter.Size = size;
        //     this.MappingSetter.DateFormat = dateFormat;
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     return default;
        // }

        // public DateTime ToDateColumn(int columnIndex, string dateFormat)
        // {
        //     this.MappingSetter.DateFormat = dateFormat;
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     return default;
        // }

        // public DateTime ToDateColumn(string columnName, string dateFormat)
        // {
        //     this.MappingSetter.DateFormat = dateFormat;
        //     this.MappingSetter.ColumnName = columnName;
        //     return default;
        // }

        // public DateTime ToDateColumn(int columnIndex, int size, string dateFormat)
        // {
        //     this.MappingSetter.DateFormat = dateFormat;
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     this.MappingSetter.Size = size;
        //     return default;
        // }

        // public DateTime? ToOptionalCulturedDateColumn(int columnIndex, string cultureName)
        // {
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     this.MappingSetter.CultureName = cultureName;
        //     return default;
        // }

        // public DateTime? ToOptionalCulturedDateColumn(string columnName, string cultureName)
        // {
        //     this.MappingSetter.ColumnName = columnName;
        //     this.MappingSetter.CultureName = cultureName;
        //     return default;
        // }

        // public DateTime? ToOptionalCulturedDateColumn(int columnIndex, string cultureName, string dateFormat)
        // {
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     this.MappingSetter.CultureName = cultureName;
        //     this.MappingSetter.DateFormat = dateFormat;
        //     return default;
        // }

        // public DateTime? ToOptionalCulturedDateColumn(string columnName, string cultureName, string dateFormat)
        // {
        //     this.MappingSetter.ColumnName = columnName;
        //     this.MappingSetter.CultureName = cultureName;
        //     this.MappingSetter.DateFormat = dateFormat;
        //     return default;
        // }

        // public DateTime? ToOptionalCulturedDateColumn(int columnIndex, int size, string cultureName)
        // {
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     this.MappingSetter.CultureName = cultureName;
        //     this.MappingSetter.Size = size;
        //     return default;
        // }

        // public DateTime? ToOptionalCulturedDateColumn(int columnIndex, int size, string cultureName, string dateFormat)
        // {
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     this.MappingSetter.CultureName = cultureName;
        //     this.MappingSetter.Size = size;
        //     this.MappingSetter.DateFormat = dateFormat;
        //     return default;
        // }

        // public DateTime? ToOptionalDateColumn(int columnIndex, string dateFormat)
        // {
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     this.MappingSetter.DateFormat = dateFormat;
        //     return default;
        // }

        // public DateTime? ToOptionalDateColumn(string columnName, string dateFormat)
        // {
        //     this.MappingSetter.ColumnName = columnName;
        //     this.MappingSetter.DateFormat = dateFormat;
        //     return default;
        // }

        // public DateTime? ToOptionalDateColumn(int columnIndex, int size, string dateFormat)
        // {
        //     this.MappingSetter.ColumnIndex = columnIndex;
        //     this.MappingSetter.Size = size;
        //     this.MappingSetter.DateFormat = dateFormat;
        //     return default;
        // }

        // public string ToSourceName()
        // {
        //     this.MappingSetter.ForSourceName = true;
        //     return default;
        // }

        // public int ToLineNumber()
        // {
        //     this.MappingSetter.ForLineNumber = true;
        //     return default;
        // }
        // public Guid ToRowGuid()
        // {
        //     this.MappingSetter.ForRowGuid = true;
        //     return default;
        // }
        // #endregion
    }
}