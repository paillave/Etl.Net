using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Paillave.Etl.Core;

namespace Paillave.Etl.TextFile.Core
{
    public class LineSerializer<T>
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
            var stringValues = this.Splitter.Split(line);
            return ObjectBuilder<T>.CreateInstance(this._indexToPropertySerializerDictionary.ToDictionary(i => i.Value.PropertyName, i => i.Value.Deserialize(stringValues[i.Key])));
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
