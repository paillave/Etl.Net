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
    public class LineColumnNameParserConfiguration<TDest> : LineIndexParserConfiguration<TDest> where TDest : new()
    {
        private readonly IDictionary<string, int> _indexDictionary;
        protected class ColumnLineProcessor : LineProcessor<TDest>
        {
            public ColumnLineProcessor(IDictionary<string, int> indexDictionary, IDictionary<int, PropertyMap> propertyDictionary, CultureInfo cultureInfo) : base(propertyDictionary, cultureInfo)
            {
            }
        }

        public LineColumnNameParserConfiguration(IDictionary<string, int> indexDictionary)
        {
            this._indexDictionary = indexDictionary;
        }
        public LineColumnNameParserConfiguration(IEnumerable<string> columnNames)
        {
            this._indexDictionary = columnNames.Select((name, idx) => new { name, idx }).ToDictionary(i => i.name, i => i.idx);
        }
        public LineColumnNameParserConfiguration<TDest> MapColumnToProperty<TField>(string columnName, Expression<Func<TDest, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            base.MapColumnToProperty(this._indexDictionary[columnName], memberLamda, cultureInfo);
            return this;
        }
        public new LineColumnNameParserConfiguration<TDest> MapColumnToProperty<TField>(int index, Expression<Func<TDest, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            base.MapColumnToProperty(index, memberLamda, cultureInfo);
            return this;
        }
        public new LineColumnNameParserConfiguration<TDest> WithGlobalCultureInfo(CultureInfo cultureInfo)
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
