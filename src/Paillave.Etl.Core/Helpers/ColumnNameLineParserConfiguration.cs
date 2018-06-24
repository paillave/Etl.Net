using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Paillave.Etl.Core.Helpers
{
    public class ColumnNameLineParserConfiguration<TDest> : IndexLineParserConfiguration<TDest> where TDest : new()
    {
        private readonly IDictionary<string, int> _indexDictionary;
        private readonly IDictionary<string, int> _ignoreCaseIndexDictionary;
        private bool _ignoreCase = true;
        public ColumnNameLineParserConfiguration(IDictionary<string, int> indexDictionary)
        {
            this._indexDictionary = indexDictionary;
            this._ignoreCaseIndexDictionary = indexDictionary.ToDictionary(i => i.Key.ToLowerInvariant(), i => i.Value);
        }
        public ColumnNameLineParserConfiguration(IEnumerable<string> columnNames)
        {
            this._indexDictionary = columnNames.Select((name, idx) => new { name, idx }).ToDictionary(i => i.name, i => i.idx);
            this._ignoreCaseIndexDictionary = _indexDictionary.ToDictionary(i => i.Key.ToLowerInvariant(), i => i.Value);
        }
        public ColumnNameLineParserConfiguration<TDest> RespectCase()
        {
            this._ignoreCase = false;
            return this;
        }
        public ColumnNameLineParserConfiguration<TDest> IgnoreCase()
        {
            this._ignoreCase = true;
            return this;
        }
        public ColumnNameLineParserConfiguration<TDest> MapColumnToProperty<TField>(string columnName, Expression<Func<TDest, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            if (this._ignoreCase)
                base.MapColumnToProperty(this._ignoreCaseIndexDictionary[columnName.ToLowerInvariant()], memberLamda, cultureInfo);
            else
                base.MapColumnToProperty(this._indexDictionary[columnName], memberLamda, cultureInfo);
            return this;
        }
        public new ColumnNameLineParserConfiguration<TDest> MapColumnToProperty<TField>(int index, Expression<Func<TDest, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            base.MapColumnToProperty(index, memberLamda, cultureInfo);
            return this;
        }
        public new ColumnNameLineParserConfiguration<TDest> WithGlobalCultureInfo(CultureInfo cultureInfo)
        {
            base.WithGlobalCultureInfo(cultureInfo);
            return this;
        }
        private PropertyInfo GetPropertyInfo<TField>(Expression<Func<TDest, TField>> memberLamda)
        {
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression != null)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null) return property;
            }
            throw new ArgumentException("Not a navigation expression", nameof(memberLamda));
        }
    }
}
