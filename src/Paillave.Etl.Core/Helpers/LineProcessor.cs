using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;

namespace Paillave.Etl.Core.Helpers
{
    public class LineProcessor<TDest> : ILineProcessor<TDest> where TDest : new()
    {
        private IDictionary<int, PropertyMap> _indexToPropertyDictionary;
        private CultureInfo _cultureInfo;
        public LineProcessor(IDictionary<int, PropertyMap> indexToPropertyDictionary, CultureInfo cultureInfo)
        {
            this._indexToPropertyDictionary = indexToPropertyDictionary;
            this._cultureInfo = cultureInfo;
        }
        protected object ParseValue(PropertyMap propDef, string value)
        {
            return propDef.TypeConverter.ConvertFromString(null, propDef.CultureInfo ?? this._cultureInfo, value);
        }

        protected void SetValue(PropertyMap propDef, object destination, object value)
        {
            propDef.PropertyInfo.SetValue(destination, value);
        }
        public void ParseAndSetValue(int index, string value, object destination)
        {
            PropertyMap propDef;
            if (_indexToPropertyDictionary.TryGetValue(index, out propDef))
                SetValue(propDef, destination, ParseValue(propDef, value));
        }

        public virtual TDest Parse(string[] values)
        {
            var destination = new TDest();
            foreach (var item in this._indexToPropertyDictionary)
                SetValue(item.Value, destination, ParseValue(item.Value, values[item.Key]));
            return destination;
        }
    }
}
