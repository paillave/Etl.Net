using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Paillave.Etl.ExcelFile.Core
{
    public class ExcelFilePropertySetter
    {
        private readonly CultureInfo _cultureInfo;
        private readonly TypeConverter _typeConverter;
        public PropertyInfo PropertyInfo { get; }
        public string ColumnName { get; }
        public string Column { get; }
        public ExcelFilePropertySetter(PropertyInfo propertyInfo, CultureInfo cultureInfo, string columnName, string column = null)
        {
            this.Column = column ?? columnName;
            this.ColumnName = columnName;
            this.PropertyInfo = propertyInfo;
            this._typeConverter = TypeDescriptor.GetConverter(this.PropertyInfo.PropertyType);
            this._cultureInfo = cultureInfo;
        }

        private object Deserialize(string text)
        {
            //TODO: Better exception handleling here
            try
            {
                
                if (PropertyInfo.PropertyType == typeof(DateTime) || PropertyInfo.PropertyType == typeof(DateTime?))
                {
                    if (string.IsNullOrWhiteSpace(text) && PropertyInfo.PropertyType == typeof(DateTime?)) return (DateTime?)null;
                    Double outDouble;
                    if (Double.TryParse(text, out outDouble))
                    {
                        return DateTime.FromOADate(outDouble);
                    }
                    return DateTime.ParseExact(text.Trim(), _cultureInfo.DateTimeFormat.LongDatePattern, _cultureInfo);
                }
                return _typeConverter.ConvertFromString(null, _cultureInfo, text.Trim());
            } catch (Exception ex)
            {
                throw new Exception($"Error reading field '{Column}' with value '{text}' for Type {PropertyInfo.PropertyType}", ex);
            }
        }

        public object ParsedValue { get; private set; } = null;
        public bool SetValue(ExcelWorksheet excelWorksheet, int row, int column)
        {
            return SetValue(excelWorksheet.GetValue(row, column));
        }
        private bool SetValue(object value)
        {
            if (value != null)
            {
                if (PropertyInfo.PropertyType != value.GetType())
                {
                    var tmpVal = value.ToString();
                    if (string.IsNullOrEmpty(tmpVal)) return false;
                    SetValue(tmpVal);
                }
                else
                {
                    this.ParsedValue = value;
                }
            }
            else
                return false;
            return true;
        }

        private void SetValue(string text)
        {
            this.ParsedValue = Deserialize(text);
        }
    }
}
