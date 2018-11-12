using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Paillave.Etl.TextFile.Core
{
    public class FlatFilePropertySerializer
    {
        private readonly CultureInfo _cultureInfo;
        private readonly TypeConverter _typeConverter;
        private readonly PropertyInfo _propertyInfo;
        public string PropertyName => _propertyInfo.Name;
        public FlatFilePropertySerializer(PropertyInfo propertyInfo, CultureInfo cultureInfo)
        {
            this._propertyInfo = propertyInfo;
            this._typeConverter = TypeDescriptor.GetConverter(this._propertyInfo.PropertyType);
            this._cultureInfo = cultureInfo;
        }

        private string Serialize(object value)
        {
            return _typeConverter.ConvertToString(null, _cultureInfo, value);
        }
        public object Deserialize(string text)
        {
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
