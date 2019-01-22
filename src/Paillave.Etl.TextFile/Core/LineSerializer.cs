using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Paillave.Etl.Core;
//https://bytefish.github.io/TinyCsvParser/index.html
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
            var values = this._indexToPropertySerializerDictionary.ToDictionary(i => i.Value.PropertyName, i =>
            {
                string valueToParse = null;
                try
                {
                    valueToParse = stringValues[i.Key];
                }
                catch (Exception ex)
                {
                    throw new FlatFileNoFieldDeserializeException(i.Key, i.Value.PropertyName, ex);
                }
                try
                {
                    return i.Value.Deserialize(valueToParse);
                }
                catch (Exception ex)
                {
                    throw new FlatFileFieldDeserializeException(i.Key, i.Value.PropertyName, valueToParse, ex);
                }
            });
            var obj = ObjectBuilder<T>.CreateInstance(values);
            return obj;
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
