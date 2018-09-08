using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Paillave.Etl.TextFile.Core
{
    public class LineSerializer<T> where T : new()
    {
        public ILineSplitter Splitter { get; }

        private readonly IDictionary<int, FlatFilePropertySerializer> _indexToPropertySerializerDictionary;

        public LineSerializer(ILineSplitter splitter, IDictionary<int, FlatFilePropertySerializer> indexToPropertySerializerDictionary)
        {
            this.Splitter = splitter;
            this._indexToPropertySerializerDictionary = indexToPropertySerializerDictionary;
        }
        public T Deserialize(string line)
        {
            T value = new T();
            var stringValues = this.Splitter.Split(line);
            foreach (var item in this._indexToPropertySerializerDictionary)
                item.Value.SetValue(value, stringValues[item.Key]);
            return value;
        }
        public string Serialize(T value)
        {
            var stringValue = _indexToPropertySerializerDictionary
                .OrderBy(i => i.Key)
                .Select(i => i.Value.GetValue(value));
            return this.Splitter.Join(stringValue);
        }
    }
}