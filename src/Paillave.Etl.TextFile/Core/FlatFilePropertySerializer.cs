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
