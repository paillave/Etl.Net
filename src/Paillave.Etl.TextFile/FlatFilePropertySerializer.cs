using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Paillave.Etl.TextFile;

public class FlatFilePropertySerializer(PropertyInfo propertyInfo, CultureInfo cultureInfo, string[] trueValues, string[] falseValues, string column)
{
    private readonly CultureInfo _cultureInfo = cultureInfo;
    private readonly TypeConverter _typeConverter = new GeneralTypeConverter(propertyInfo.PropertyType, trueValues, falseValues);
    private readonly PropertyInfo _propertyInfo = propertyInfo;
    private readonly bool _isTargetString = propertyInfo.PropertyType == typeof(string);
    public string Column { get; } = column;
    public string PropertyName => _propertyInfo.Name;

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
