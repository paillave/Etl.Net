using System;
using System.Collections.Generic;
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
        public FlatFilePropertySerializer(PropertyInfo propertyInfo, CultureInfo cultureInfo, string[] trueValues, string[] falseValues, string column)
        {
            this._propertyInfo = propertyInfo;
            this._typeConverter = new GeneralTypeConverter(propertyInfo.PropertyType, trueValues, falseValues);
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
            return _typeConverter.ConvertFromString(null, _cultureInfo, text.Trim());
        }

        public string GetValue(object target)
        {
            return Serialize(_propertyInfo.GetValue(target));
        }
        
    }
}
