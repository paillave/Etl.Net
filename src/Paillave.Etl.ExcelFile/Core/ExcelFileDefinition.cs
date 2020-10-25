using OfficeOpenXml;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Mapping;
using Paillave.Etl.Core.Mapping.Visitors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Paillave.Etl.ExcelFile.Core
{
    public enum DataOrientation
    {
        Horizontal,
        Vertical
    }
    public static class ExcelFileDefinition
    {
        public static ExcelFileDefinition<T> Create<T>(Expression<Func<IFieldMapper, T>> expression) => new ExcelFileDefinition<T>().WithMap(expression);
    }
    public class ExcelFileDefinition<T>
    {
        private IList<ExcelFileFieldDefinition> _fieldDefinitions = new List<ExcelFileFieldDefinition>();
        private CultureInfo _cultureInfo = CultureInfo.CurrentCulture;
        private ExcelAddressBase _columnHeaderRange = null;
        private ExcelAddressBase _dataRange = null;
        private DataOrientation _datasetOrientation = DataOrientation.Vertical;

        public ExcelFileDefinition<T> WithHorizontalDataset()
        {
            _datasetOrientation = DataOrientation.Horizontal;
            return this;
        }

        public ExcelFileDefinition<T> WithVerticalDataset()
        {
            _datasetOrientation = DataOrientation.Vertical;
            return this;
        }

        public ExcelFileDefinition<T> HasColumnHeader(string stringAddress)
        {
            _columnHeaderRange = new ExcelAddressBase(stringAddress);
            return this;
        }
        public ExcelFileDefinition<T> WithDataset(string stringAddress)
        {
            _dataRange = new ExcelAddressBase(stringAddress);
            return this;
        }
        public ExcelFileDefinition<T> WithMap(Expression<Func<IFieldMapper, T>> expression)
        {
            MapperVisitor vis = new MapperVisitor();
            vis.Visit(expression);
            foreach (var item in vis.MappingSetters)
            {
                this.SetFieldDefinition(new ExcelFileFieldDefinition
                {
                    ColumnName = item.ColumnName,
                    Position = item.ColumnIndex,
                    PropertyInfo = item.TargetPropertyInfo,
                    CultureInfo = item.CreateCultureInfo()
                });
            }
            return this;
        }
        public ExcelFileDefinition<T> SetDefaultMapping(bool withColumnHeader = true, CultureInfo cultureInfo = null)
        {
            foreach (var item in typeof(T).GetProperties().Select((propertyInfo, index) => new { propertyInfo = propertyInfo, Position = index }))
            {
                SetFieldDefinition(new ExcelFileFieldDefinition
                {
                    CultureInfo = cultureInfo,
                    ColumnName = withColumnHeader ? item.propertyInfo.Name : null,
                    Position = item.Position,
                    PropertyInfo = item.propertyInfo
                });
            }
            return this;
        }
        public ExcelFileReader GetExcelReader(ExcelWorksheet excelWorksheet = null)
        {
            if ((_fieldDefinitions?.Count ?? 0) == 0) SetDefaultMapping();
            IEnumerable<string> columnNames;
            if (excelWorksheet == null) columnNames = GetDefaultColumnNames().ToList();
            else columnNames = GetColumnNames(excelWorksheet);
            if (this._columnHeaderRange != null)
            {
                var dico = _fieldDefinitions.Join(
                    columnNames.Select((ColumnName, Position) => new { ColumnName, Position }),
                    i => i.ColumnName.Trim(),
                    i => i.ColumnName.Trim(),
                    (fd, po) => new
                    {
                        Position = po.Position,
                        PropertySerializer = new ExcelFilePropertySetter(fd.PropertyInfo, fd.CultureInfo ?? _cultureInfo, fd.ColumnName)
                    })
                    .ToDictionary(i => i.Position, i => i.PropertySerializer);
                return new ExcelFileReader(dico, _columnHeaderRange, _dataRange, _datasetOrientation);
            }
            else
            {
                var dico = _fieldDefinitions
                    .OrderBy(i => i.Position)
                    .Select((fd, idx) => new
                    {
                        Position = idx,
                        PropertySerializer = new ExcelFilePropertySetter(fd.PropertyInfo, fd.CultureInfo ?? _cultureInfo, fd.ColumnName, $"<{idx}>")
                    })
                    .ToDictionary(i => i.Position, i => i.PropertySerializer);
                return new ExcelFileReader(dico, _columnHeaderRange, _dataRange, _datasetOrientation);
            }
        }
        private IEnumerable<string> GetColumnNames(ExcelWorksheet excelWorksheet)
        {
            //TODO: better exception handleling here
            if (_columnHeaderRange == null) return new string[] { };
            if (_columnHeaderRange.Columns == 1)
            {
                if (_datasetOrientation != DataOrientation.Horizontal) return new string[] { };
                else return Enumerable
                        .Range(_columnHeaderRange.Start.Row, _columnHeaderRange.End.Row - _columnHeaderRange.Start.Row + 1)
                        .Select(i => excelWorksheet.GetValue<string>(i, _columnHeaderRange.Start.Column));
            }
            if (_columnHeaderRange.Rows == 1)
            {
                if (_datasetOrientation != DataOrientation.Vertical) return new string[] { };
                else return Enumerable
                        .Range(_columnHeaderRange.Start.Column, _columnHeaderRange.End.Column - _columnHeaderRange.Start.Column + 1)
                        .Select(i => excelWorksheet.GetValue<string>(_columnHeaderRange.Start.Row, i));
            }
            return new string[] { };
        }
        private IEnumerable<string> GetDefaultColumnNames()
        {
            return _fieldDefinitions.Select((i, idx) => new { Name = i.ColumnName ?? i.PropertyInfo.Name, DefinedPosition = i.Position, FallbackPosition = idx })
                .OrderBy(i => i.DefinedPosition)
                .ThenBy(i => i.FallbackPosition)
                .Select(i => i.Name);
        }
        public ExcelFileDefinition<T> WithCultureInfo(CultureInfo cultureInfo)
        {
            this._cultureInfo = cultureInfo;
            return this;
        }
        public ExcelFileDefinition<T> WithCultureInfo(string name)
        {
            this._cultureInfo = CultureInfo.GetCultureInfo(name);
            return this;
        }
        public ExcelFileDefinition<T> MapColumnToProperty<TField>(int index, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            SetFieldDefinition(new ExcelFileFieldDefinition
            {
                CultureInfo = cultureInfo,
                Position = index,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        public ExcelFileDefinition<T> MapColumnToProperty<TField>(int index, Expression<Func<T, TField>> memberLamda, string cultureInfo)
        {
            SetFieldDefinition(new ExcelFileFieldDefinition
            {
                CultureInfo = CultureInfo.GetCultureInfo(cultureInfo),
                Position = index,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        private void SetFieldDefinition(ExcelFileFieldDefinition fieldDefinition)
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
        public ExcelFileDefinition<T> MapColumnToProperty<TField>(string columnName, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            SetFieldDefinition(new ExcelFileFieldDefinition
            {
                CultureInfo = cultureInfo,
                ColumnName = columnName,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        public ExcelFileDefinition<T> MapColumnToProperty<TField>(string columnName, Expression<Func<T, TField>> memberLamda, string cultureInfo)
        {
            SetFieldDefinition(new ExcelFileFieldDefinition
            {
                CultureInfo = CultureInfo.GetCultureInfo(cultureInfo),
                ColumnName = columnName,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        public ExcelFileDefinition<T> MapColumnToProperty<TField>(string columnName, int position, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            SetFieldDefinition(new ExcelFileFieldDefinition
            {
                CultureInfo = cultureInfo,
                ColumnName = columnName,
                Position = position,
                PropertyInfo = memberLamda.GetPropertyInfo()
            });
            return this;
        }
        public ExcelFileDefinition<T> MapColumnToProperty<TField>(string columnName, int position, Expression<Func<T, TField>> memberLamda, string cultureInfo)
        {
            SetFieldDefinition(new ExcelFileFieldDefinition
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
