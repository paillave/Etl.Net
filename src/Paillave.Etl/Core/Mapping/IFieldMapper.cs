using System;

namespace Paillave.Etl.Core.Mapping
{
    public interface IFieldMapper
    {
        DateTime ToDateColumn(int columnIndex, string dateFormat);
        DateTime ToCulturedDateColumn(int columnIndex, string cultureName);
        DateTime ToCulturedDateColumn(int columnIndex, string cultureName, string dateFormat);
        DateTime? ToOptionalDateColumn(int columnIndex, string dateFormat);
        DateTime? ToOptionalCulturedDateColumn(int columnIndex, string cultureName);
        DateTime? ToOptionalCulturedDateColumn(int columnIndex, string cultureName, string dateFormat);
        T ToNumberColumn<T>(int columnIndex, string decimalSeparator);
        T ToNumberColumn<T>(int columnIndex, string decimalSeparator, string groupSeparator);
        T ToColumn<T>(int columnIndex);
        string ToColumn(int columnIndex);

        DateTime ToDateColumn(int columnIndex, int size, string dateFormat);
        DateTime ToCulturedDateColumn(int columnIndex, int size, string cultureName);
        DateTime ToCulturedDateColumn(int columnIndex, int size, string cultureName, string dateFormat);
        DateTime? ToOptionalDateColumn(int columnIndex, int size, string dateFormat);
        DateTime? ToOptionalCulturedDateColumn(int columnIndex, int size, string cultureName);
        DateTime? ToOptionalCulturedDateColumn(int columnIndex, int size, string cultureName, string dateFormat);
        T ToNumberColumn<T>(int columnIndex, int size, string decimalSeparator);
        T ToNumberColumn<T>(int columnIndex, int size, string decimalSeparator, string groupSeparator);
        T ToColumn<T>(int columnIndex, int size);
        string ToColumn(int columnIndex, int size);


        DateTime ToDateColumn(string columnName, string dateFormat);
        DateTime ToCulturedDateColumn(string columnName, string cultureName);
        DateTime ToCulturedDateColumn(string columnName, string cultureName, string dateFormat);
        DateTime? ToOptionalDateColumn(string columnName, string dateFormat);
        DateTime? ToOptionalCulturedDateColumn(string columnName, string cultureName);
        DateTime? ToOptionalCulturedDateColumn(string columnName, string cultureName, string dateFormat);
        T ToNumberColumn<T>(string columnName, string decimalSeparator);
        T ToNumberColumn<T>(string columnName, string decimalSeparator, string groupSeparator);
        T ToColumn<T>(string columnName);
        string ToColumn(string columnName);

        bool ToBooleanColumn(string columnName, string trueValue, string falseValue);
        bool ToBooleanColumn(string columnName, string[] trueValues, string[] falseValues);
        bool? ToOptionalBooleanColumn(string columnName, string trueValue, string falseValue);
        bool? ToOptionalBooleanColumn(string columnName, string[] trueValues, string[] falseValues);

        string ToSourceName();
        int ToLineNumber();
        Guid ToRowGuid();
    }
}