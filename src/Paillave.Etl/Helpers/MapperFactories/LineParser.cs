using System;
using System.Linq;
using System.Collections.Generic;

namespace Paillave.Etl.Helpers.MapperFactories
{
    public class LineParser<TDest>
    {
        private IDictionary<int, PropertyMapper> _indexToPropertyDictionary;
        private Func<TDest> _constructor;
        public LineParser(IDictionary<int, PropertyMapper> indexToPropertyDictionary, Func<TDest> constructor)
        {
            this._indexToPropertyDictionary = indexToPropertyDictionary;
            this._constructor = constructor;
        }
        private object ParseValue(PropertyMapper propDef, string value)
        {
            return propDef.TypeConverter.ConvertFromString(null, propDef.CultureInfo, value);
        }
        private string SerializeValue(PropertyMapper propDef, object value)
        {
            return propDef.TypeConverter.ConvertToString(null, propDef.CultureInfo, value);
        }

        private void SetValue(PropertyMapper propDef, object destination, object value)
        {
            propDef.PropertyInfo.SetValue(destination, value);
        }
        private void ParseAndSetValue(int index, string value, object destination)
        {
            PropertyMapper propDef;
            if (_indexToPropertyDictionary.TryGetValue(index, out propDef))
                SetValue(propDef, destination, ParseValue(propDef, value));
        }
        private string GetValueAndSerialize(PropertyMapper propDef, object source)
        {
            return SerializeValue(propDef, propDef.PropertyInfo.GetValue(source));
        }

        public virtual TDest Parse(IList<string> values)
        {
            var destination = this._constructor();
            foreach (var item in this._indexToPropertyDictionary)
                SetValue(item.Value, destination, ParseValue(item.Value, values[item.Key]));
            return destination;
        }
        public virtual IList<string> Serialize(TDest value)
        {
            return this._indexToPropertyDictionary.OrderBy(i => i.Key).Select(i => GetValueAndSerialize(i.Value, value)).ToList();
        }
    }
}
