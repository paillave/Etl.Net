using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.TextFile.Core
{
    public class FlatFileDefinition<T> where T : new()
    {
        public bool HasColumnHeader => _fieldDefinitions.Any(i => !string.IsNullOrWhiteSpace(i.ColumnName));
        private IList<FlatFileFieldDefinition> _fieldDefinitions = new List<FlatFileFieldDefinition>();
        private ILineSplitter _lineSplitter = new ColumnSeparatorLineSplitter();
        private CultureInfo _cultureInfo = CultureInfo.CurrentCulture;
        public int FirstLinesToIgnore { get; private set; }
        public FlatFileDefinition<T> IgnoreFirstLines(int firstLinesToIgnore)
        {
            FirstLinesToIgnore = firstLinesToIgnore;
            return this;
        }

        public FlatFileDefinition<T> SetDefaultMapping(bool withColumnHeader = true, CultureInfo cultureInfo = null)
        {
            foreach (var item in typeof(T).GetProperties().Select((propertyInfo, index) => new { propertyInfo = propertyInfo, Position = index }))
            {
                SetFieldDefinition(new FlatFileFieldDefinition
                {
                    CultureInfo = cultureInfo,
                    ColumnName = withColumnHeader ? item.propertyInfo.Name : null,
                    Position = item.Position,
                    PropertyInfo = item.propertyInfo
                });
            }
            return this;
        }
        public LineSerializer<T> GetSerializer(string headerLine)
        {
            return GetSerializer(_lineSplitter.Split(headerLine));
        }
        public LineSerializer<T> GetSerializer(IEnumerable<string> columnNames = null)
        {
            if ((_fieldDefinitions?.Count ?? 0) == 0) SetDefaultMapping();
            if (columnNames == null) columnNames = GetDefaultColumnNames().ToList();
            if (this.HasColumnHeader)
            {
                var indexToPropertySerializerDictionary = _fieldDefinitions.Join(
                    columnNames.Select((ColumnName, Position) => new { ColumnName, Position }),
                    i => i.ColumnName.Trim(),
                    i => i.ColumnName.Trim(),
                    (fd, po) => new
                    {
                        Position = po.Position,
                        PropertySerializer = new FlatFilePropertySerializer(fd.PropertyInfo, fd.CultureInfo ?? _cultureInfo)
                    })
                    .OrderBy(i => i.Position)
                    .Select((i, idx) => new
                    {
                        Position = idx,
                        PropertySerializer = i.PropertySerializer
                    })
                    .ToDictionary(i => i.Position, i => i.PropertySerializer);

                return new LineSerializer<T>(_lineSplitter, indexToPropertySerializerDictionary);
            }
            else
            {
                var indexToPropertySerializerDictionary = _fieldDefinitions
                    .OrderBy(i => i.Position)
                    .Select((fd, idx) => new
                    {
                        Position = idx,
                        PropertySerializer = new FlatFilePropertySerializer(fd.PropertyInfo, fd.CultureInfo ?? _cultureInfo)
                    })
                    .ToDictionary(i => i.Position, i => i.PropertySerializer);
                return new LineSerializer<T>(_lineSplitter, indexToPropertySerializerDictionary);
            }
        }
        private IEnumerable<string> GetDefaultColumnNames()
        {
            return _fieldDefinitions.Select((i, idx) => new { Name = i.ColumnName ?? i.PropertyInfo.Name, DefinedPosition = i.Position, FallbackPosition = idx })
                .OrderBy(i => i.DefinedPosition)
                .ThenBy(i => i.FallbackPosition)
                .Select(i => i.Name);
        }
        public string GenerateDefaultHeaderLine()
        {
            return _lineSplitter.Join(GetDefaultColumnNames());
        }
        public FlatFileDefinition<T> IsColumnSeparated(char separator = ';', char textDelimiter = '"')
        {
            this._lineSplitter = new ColumnSeparatorLineSplitter(separator, textDelimiter);
            return this;
        }
        public FlatFileDefinition<T> HasFixedColumnWidth(params int[] columnSizes)
        {
            this._lineSplitter = new FixedColumnWidthLineSplitter(columnSizes);
            return this;
        }
        public FlatFileDefinition<T> WithCultureInfo(CultureInfo cultureInfo)
        {
            this._cultureInfo = cultureInfo;
            return this;
        }
        public FlatFileDefinition<T> WithCultureInfo(string name)
        {
            this._cultureInfo = CultureInfo.GetCultureInfo(name);
            return this;
        }
        public FlatFileDefinition<T> MapColumnToProperty<TField>(int index, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            SetFieldDefinition(new FlatFileFieldDefinition
            {
                CultureInfo = cultureInfo,
                Position = index,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        public FlatFileDefinition<T> MapColumnToProperty<TField>(int index, Expression<Func<T, TField>> memberLamda, string cultureInfo)
        {
            SetFieldDefinition(new FlatFileFieldDefinition
            {
                CultureInfo = CultureInfo.GetCultureInfo(cultureInfo),
                Position = index,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        private void SetFieldDefinition(FlatFileFieldDefinition fieldDefinition)
        {
            var existingFieldDefinition = _fieldDefinitions.FirstOrDefault(i => i.PropertyInfo.Name == fieldDefinition.PropertyInfo.Name);
            if (existingFieldDefinition == null)
            {
                if (fieldDefinition.Position == null)
                    fieldDefinition.Position = (_fieldDefinitions.Max(i => i.Position) ?? 0) + 1;
                _fieldDefinitions.Add(fieldDefinition);
            }
            else
            {
                if (fieldDefinition.ColumnName != null) existingFieldDefinition.ColumnName = fieldDefinition.ColumnName;
                if (fieldDefinition.Position != null) existingFieldDefinition.Position = fieldDefinition.Position;
            }
        }
        public FlatFileDefinition<T> MapColumnToProperty<TField>(string columnName, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            SetFieldDefinition(new FlatFileFieldDefinition
            {
                CultureInfo = cultureInfo,
                ColumnName = columnName,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        public FlatFileDefinition<T> MapColumnToProperty<TField>(string columnName, Expression<Func<T, TField>> memberLamda, string cultureInfo)
        {
            SetFieldDefinition(new FlatFileFieldDefinition
            {
                CultureInfo = CultureInfo.GetCultureInfo(cultureInfo),
                ColumnName = columnName,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        public FlatFileDefinition<T> MapColumnToProperty<TField>(string columnName, int position, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            SetFieldDefinition(new FlatFileFieldDefinition
            {
                CultureInfo = cultureInfo,
                ColumnName = columnName,
                Position = position,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        public FlatFileDefinition<T> MapColumnToProperty<TField>(string columnName, int position, Expression<Func<T, TField>> memberLamda, string cultureInfo)
        {
            SetFieldDefinition(new FlatFileFieldDefinition
            {
                CultureInfo = CultureInfo.GetCultureInfo(cultureInfo),
                ColumnName = columnName,
                Position = position,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
    }
}
