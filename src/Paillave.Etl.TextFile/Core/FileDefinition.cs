using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.TextFile.Core
{
    public class FileDefinition<T> where T : new()
    {
        public bool ReliesOnHeader => _fieldDefinitions.Any(i => !string.IsNullOrWhiteSpace(i.Name));
        private IList<FieldDefinition> _fieldDefinitions = new List<FieldDefinition>();
        private ILineSplitter _lineSplitter = new ColumnSeparatorLineSplitter();
        private string _headerLine = null;
        private CultureInfo _cultureInfo = CultureInfo.CurrentCulture;
        public int FirstLinesToIgnore { get; private set; }
        public FileDefinition<T> IgnoreFirstLines(int firstLinesToIgnore)
        {
            FirstLinesToIgnore = firstLinesToIgnore;
            return this;
        }
        public LineSerializer<T> GetSerializer()
        {
            var indexToPropertySerializerDictionary = _fieldDefinitions.GroupJoin(
                _lineSplitter
                    .Split(_headerLine ?? string.Empty)
                    .Select((Name, Position) => new { Name, Position }),
                i => i.Name,
                i => i.Name,
                (fd, po) => new
                {
                    Index = fd.Position ?? po.FirstOrDefault().Position,
                    PropertySerializer = new PropertySerializer(fd.PropertyInfo, fd.CultureInfo)
                })
                .ToDictionary(i => i.Index, i => i.PropertySerializer);

            return new LineSerializer<T>(_lineSplitter, indexToPropertySerializerDictionary);
        }

        public FileDefinition<T> IsColumnSeparated(char separator = ';', char textDelimiter = '"')
        {
            this._lineSplitter = new ColumnSeparatorLineSplitter(separator, textDelimiter);
            return this;
        }
        public FileDefinition<T> HasFixedColumnWidth(params int[] columnSizes)
        {
            this._lineSplitter = new FixedColumnWidthLineSplitter(columnSizes);
            return this;
        }
        public FileDefinition<T> WithHeaderLine(string headerLine)
        {
            _headerLine = headerLine;
            return this;
        }
        public FileDefinition<T> WithCultureInfo(CultureInfo cultureInfo)
        {
            this._cultureInfo = cultureInfo;
            return this;
        }
        public FileDefinition<T> WithCultureInfo(string name)
        {
            this._cultureInfo = CultureInfo.GetCultureInfo(name);
            return this;
        }
        public FileDefinition<T> MapColumnToProperty<TField>(int index, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            SetFieldDefinition(new FieldDefinition
            {
                CultureInfo = cultureInfo,
                Position = index,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        public FileDefinition<T> MapColumnToProperty<TField>(int index, Expression<Func<T, TField>> memberLamda, string cultureInfo)
        {
            SetFieldDefinition(new FieldDefinition
            {
                CultureInfo = CultureInfo.GetCultureInfo(cultureInfo),
                Position = index,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        private void SetFieldDefinition(FieldDefinition fieldDefinition)
        {
            Func<FieldDefinition, bool> criteria = i => false;
            if (fieldDefinition.Position != null)
                criteria = i => i.Position.Value == fieldDefinition.Position.Value;
            else
                criteria = i => i.Name == fieldDefinition.Name;
            var previous = _fieldDefinitions.FirstOrDefault(criteria);
            if (previous != null) _fieldDefinitions.Remove(previous);
            _fieldDefinitions.Add(fieldDefinition);
        }
        public FileDefinition<T> MapColumnToProperty<TField>(string name, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            SetFieldDefinition(new FieldDefinition
            {
                CultureInfo = cultureInfo,
                Name = name,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        public FileDefinition<T> MapColumnToProperty<TField>(string name, Expression<Func<T, TField>> memberLamda, string cultureInfo)
        {
            SetFieldDefinition(new FieldDefinition
            {
                CultureInfo = CultureInfo.GetCultureInfo(cultureInfo),
                Name = name,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
    }
}
