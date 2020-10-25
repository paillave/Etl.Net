using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Paillave.Etl.TextFile.Core
{
    public class FlatFilePropertySerializer
    {
        private readonly CultureInfo _cultureInfo;
        private readonly TypeConverter _typeConverter;
        private readonly PropertyInfo _propertyInfo;
        private readonly bool _isTargetString;
        public string Column { get; }
        public string PropertyName => _propertyInfo.Name;
        private readonly string[] _trueValues;
        private readonly string[] _falseValues;
        public FlatFilePropertySerializer(PropertyInfo propertyInfo, CultureInfo cultureInfo, string[] trueValues, string[] falseValues, string column)
        {
            this._falseValues = falseValues ?? new string[] { };
            this._trueValues = trueValues ?? new string[] { };
            this._propertyInfo = propertyInfo;
            this._typeConverter = TypeDescriptor.GetConverter(this._propertyInfo.PropertyType);
            this._cultureInfo = cultureInfo;
            this._isTargetString = propertyInfo.PropertyType == typeof(string);
            this.Column = column;
        }
        private string Serialize(object value)
        {
            return _typeConverter.ConvertToString(null, _cultureInfo, value);
        }
        public object Deserialize(string text)
        {
            if (_isTargetString && string.IsNullOrWhiteSpace(text)) return (string)null;
            //TODO: Handle deserialization pb
            if (_propertyInfo.PropertyType == typeof(DateTime))
            {
                return DateTime.ParseExact(text.Trim(), _cultureInfo.DateTimeFormat.LongDatePattern, _cultureInfo);
            }
            if (_propertyInfo.PropertyType == typeof(DateTime?))
            {
                if (string.IsNullOrWhiteSpace(text)) return (DateTime?)null;
                return DateTime.ParseExact(text.Trim(), _cultureInfo.DateTimeFormat.LongDatePattern, _cultureInfo);
            }
            if (_propertyInfo.PropertyType == typeof(bool) && (_trueValues.Length > 0 || _falseValues.Length > 0))
            {
                if (_trueValues.Contains(text.Trim())) return true;
                if (_falseValues.Contains(text.Trim())) return false;
            }
            return _typeConverter.ConvertFromString(null, _cultureInfo, text.Trim());
        }

        // public void SetValue(object target, string text)
        // {
        //     _propertyInfo.SetValue(target, Deserialize(text));
        // }

        public string GetValue(object target)
        {
            return Serialize(_propertyInfo.GetValue(target));
        }
    }
}
