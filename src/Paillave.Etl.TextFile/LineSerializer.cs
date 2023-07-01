using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Paillave.Etl.Core;
//https://bytefish.github.io/TinyCsvParser/index.html
namespace Paillave.Etl.TextFile
{
    public class LineSerializer<T>
    {
        public ILineSplitter Splitter { get; }

        private readonly IDictionary<int, FlatFilePropertySerializer> _indexToPropertySerializerDictionary;
        private readonly IEnumerable<string> _fileNamePropertyNames;
        private readonly IEnumerable<string> _rowNumberPropertyNames;
        private readonly IEnumerable<string> _rowGuidPropertyNames;

        public LineSerializer(ILineSplitter splitter, IDictionary<int, FlatFilePropertySerializer> indexToPropertySerializerDictionary, IEnumerable<string> fileNamePropertyNames, IEnumerable<string> rowNumberPropertyNames, IEnumerable<string> rowGuidPropertyNames)
        {
            this.Splitter = splitter;
            this._indexToPropertySerializerDictionary = indexToPropertySerializerDictionary;
            this._fileNamePropertyNames = fileNamePropertyNames;
            this._rowNumberPropertyNames = rowNumberPropertyNames;
            this._rowGuidPropertyNames = rowGuidPropertyNames;
        }
        public T Deserialize(string line, string sourceName, int rowNumber)
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
                // if(string.IsNullOrWhiteSpace(valueToParse) && Nullable.)
                try
                {
                    return i.Value.Deserialize(valueToParse);
                } 
                catch (Exception ex)
                {
                    throw new FlatFileFieldDeserializeException(i.Key, i.Value.PropertyName, valueToParse, ex);
                }
            });
            foreach (var fileNamePropertyName in _fileNamePropertyNames ?? new string[] { })
            {
                values[fileNamePropertyName] = sourceName;
            }
            foreach (var rowNumberPropertyName in _rowNumberPropertyNames ?? new string[] { })
            {
                values[rowNumberPropertyName] = rowNumber;
            }
            foreach (var rowGuidPropertyName in _rowGuidPropertyNames ?? new string[] { })
            {
                values[rowGuidPropertyName] = Guid.NewGuid();
            }
            var obj = ObjectBuilder<T>.CreateInstance(values);
            return obj;
        }
        public Dictionary<string, string> GetTextMapping()
        {
            var values = this._indexToPropertySerializerDictionary.Values.ToDictionary(i => i.PropertyName, i => i.Column);
            foreach (var fileNamePropertyName in _fileNamePropertyNames ?? new string[] { })
            {
                values[fileNamePropertyName] = "<SourceName>";
            }
            foreach (var rowNumberPropertyName in _rowNumberPropertyNames ?? new string[] { })
            {
                values[rowNumberPropertyName] = "<RowNumber>";
            }
            foreach (var rowGuidPropertyName in _rowGuidPropertyNames ?? new string[] { })
            {
                values[rowGuidPropertyName] = "<Guid>";
            }
            return values;
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
