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
        private CultureInfo _cultureInfo = CultureInfo.CurrentCulture;
        public int FirstLinesToIgnore { get; private set; }
        public FileDefinition<T> IgnoreFirstLines(int firstLinesToIgnore)
        {
            FirstLinesToIgnore = firstLinesToIgnore;
            return this;
        }
        public LineSerializer<T> GetSerializer(string headerLine = null)
        {
            var indexToPropertySerializerDictionary = _fieldDefinitions.GroupJoin(
                _lineSplitter
                    .Split(headerLine ?? GenerateDefaultHeaderLine())
                    .Select((Name, Position) => new { Name, Position }),
                i => i.PropertyInfo.Name,
                i => i.Name,
                (fd, po) => new
                {
                    Index = fd.Position ?? po.FirstOrDefault()?.Position,
                    Name = fd.Name,
                    PropertySerializer = new PropertySerializer(fd.PropertyInfo, fd.CultureInfo ?? _cultureInfo)
                })
                .Join(
                    typeof(T).GetProperties().Select((i, idx) => new { i.Name, Position = idx }),
                    i => i.Name,
                    i => i.Name,
                    (l, r) => new
                    {
                        Position = l.Index,
                        PropertyPosition = r.Position,
                        PropertySerializer = l.PropertySerializer
                    }
                )
                .OrderBy(i => i.Position)
                .ThenBy(i => i.PropertyPosition)
                .Select((i, idx) => new { Index = idx, i.PropertySerializer })
                .ToDictionary(i => i.Index, i => i.PropertySerializer);

            return new LineSerializer<T>(_lineSplitter, indexToPropertySerializerDictionary);
        }
        public string GenerateDefaultHeaderLine()
        {
            return _lineSplitter.Join(_fieldDefinitions.Select((i, idx) => new { Name = i.Name ?? i.PropertyInfo.Name, DefinedPosition = i.Position, FallbackPosition = idx })
                .OrderBy(i => i.DefinedPosition ?? int.MaxValue)
                .ThenBy(i => i.FallbackPosition)
                .Select(i => i.Name));
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
            var existingFieldDefinition = _fieldDefinitions.FirstOrDefault(i => i.PropertyInfo.Name == fieldDefinition.PropertyInfo.Name);
            if (existingFieldDefinition == null)
            {
                _fieldDefinitions.Add(fieldDefinition);
            }
            else
            {
                if (fieldDefinition.Name != null) existingFieldDefinition.Name = fieldDefinition.Name;
                if (fieldDefinition.Position != null) existingFieldDefinition.Position = fieldDefinition.Position;
            }
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
