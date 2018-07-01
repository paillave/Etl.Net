using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.Helpers.MapperFactories
{
    //public class ColumnNameLineParserConfiguration<TDest>: ILineParserFactory<TDest> where TDest : new()
    //{
    //    internal Dictionary<int, PropertyMap> IndexToPropertyDictionary { get; } = new Dictionary<int, PropertyMap>();
    //    protected CultureInfo CultureInfo { get; private set; }
    //    private readonly IDictionary<string, int> _indexDictionary;
    //    private readonly IDictionary<string, int> _ignoreCaseIndexDictionary;
    //    private bool _ignoreCase = true;
    //    public ColumnNameLineParserConfiguration(IDictionary<string, int> indexDictionary)
    //    {
    //        this._indexDictionary = indexDictionary;
    //        this._ignoreCaseIndexDictionary = indexDictionary.ToDictionary(i => i.Key.ToLowerInvariant(), i => i.Value);
    //    }
    //    public ColumnNameLineParserConfiguration(IEnumerable<string> columnNames)
    //    {
    //        this._indexDictionary = columnNames.Select((name, idx) => new { name, idx }).ToDictionary(i => i.name, i => i.idx);
    //        this._ignoreCaseIndexDictionary = _indexDictionary.ToDictionary(i => i.Key.ToLowerInvariant(), i => i.Value);
    //    }
    //    public ColumnNameLineParserConfiguration<TDest> RespectCase()
    //    {
    //        this._ignoreCase = false;
    //        return this;
    //    }
    //    public ColumnNameLineParserConfiguration<TDest> IgnoreCase()
    //    {
    //        this._ignoreCase = true;
    //        return this;
    //    }



    //    public ColumnNameLineParserConfiguration<TDest> MapColumnToProperty<TField>(int index, Expression<Func<TDest, TField>> memberLamda, CultureInfo cultureInfo = null)
    //    {
    //        PropertyInfo propertyInfo = this.GetPropertyInfo(memberLamda);
    //        this.IndexToPropertyDictionary[index] = new PropertyMap
    //        {
    //            PropertyInfo = propertyInfo,
    //            TypeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType),
    //            CultureInfo = cultureInfo
    //        };
    //        return this;
    //    }
    //    public ColumnNameLineParserConfiguration<TDest> WithGlobalCultureInfo(CultureInfo cultureInfo)
    //    {
    //        this.CultureInfo = cultureInfo;
    //        return this;
    //    }


    //    public ColumnNameLineParserConfiguration<TDest> MapColumnToProperty<TField>(string columnName, Expression<Func<TDest, TField>> memberLamda, CultureInfo cultureInfo = null)
    //    {
    //        int index = this._ignoreCase ? this._ignoreCaseIndexDictionary[columnName.ToLowerInvariant()] : this._indexDictionary[columnName];
    //        this.MapColumnToProperty(index, memberLamda, cultureInfo);
    //        return this;
    //    }

    //    private PropertyInfo GetPropertyInfo<TField>(Expression<Func<TDest, TField>> memberLamda)
    //    {
    //        var memberSelectorExpression = memberLamda.Body as MemberExpression;
    //        if (memberSelectorExpression != null)
    //        {
    //            var property = memberSelectorExpression.Member as PropertyInfo;
    //            if (property != null) return property;
    //        }
    //        throw new ArgumentException("Not a navigation expression", nameof(memberLamda));
    //    }

    //    public Func<IList<string>, TDest> GetLineParser()
    //    {
    //        return new LineParser<TDest>(this.IndexToPropertyDictionary, this.CultureInfo).Parse;
    //    }
    //}

    //public class IndexLineParserConfiguration<TDest> : ILineParserFactory<TDest> where TDest : new()
    //{
    //    internal Dictionary<int, PropertyMap> IndexToPropertyDictionary { get; } = new Dictionary<int, PropertyMap>();
    //    protected CultureInfo CultureInfo { get; private set; }
    //    public IndexLineParserConfiguration<TDest> MapColumnToProperty<TField>(int index, Expression<Func<TDest, TField>> memberLamda, CultureInfo cultureInfo = null)
    //    {
    //        PropertyInfo propertyInfo = this.GetPropertyInfo(memberLamda);
    //        this.IndexToPropertyDictionary[index] = new PropertyMap
    //        {
    //            PropertyInfo = propertyInfo,
    //            TypeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType),
    //            CultureInfo = cultureInfo
    //        };
    //        return this;
    //    }
    //    public IndexLineParserConfiguration<TDest> WithGlobalCultureInfo(CultureInfo cultureInfo)
    //    {
    //        this.CultureInfo = cultureInfo;
    //        return this;
    //    }
    //    private PropertyInfo GetPropertyInfo<TField>(Expression<Func<TDest, TField>> memberLamda)
    //    {
    //        var memberSelectorExpression = memberLamda.Body as MemberExpression;
    //        if (memberSelectorExpression != null)
    //        {
    //            var property = memberSelectorExpression.Member as PropertyInfo;
    //            if (property != null) return property;
    //        }
    //        throw new ArgumentException("Not a navigation expression", nameof(memberLamda));
    //    }
    //    public Func<IList<string>, TDest> GetLineParser()
    //    {
    //        return new LineParser<TDest>(this.IndexToPropertyDictionary, this.CultureInfo).Parse;
    //    }
    //}

    public class LineParser<TDest>
    {
        private IDictionary<int, PropertyMapper> _indexToPropertyDictionary;
        private Func<TDest> _constructor;
        public LineParser(IDictionary<int, PropertyMapper> indexToPropertyDictionary, Func<TDest> constructor)
        {
            this._indexToPropertyDictionary = indexToPropertyDictionary;
            this._constructor = constructor;
        }
        private object ParseValue(PropertyMapper propDef, string value)
        {
            return propDef.TypeConverter.ConvertFromString(null, propDef.CultureInfo, value);
        }

        private void SetValue(PropertyMapper propDef, object destination, object value)
        {
            propDef.PropertyInfo.SetValue(destination, value);
        }
        private void ParseAndSetValue(int index, string value, object destination)
        {
            PropertyMapper propDef;
            if (_indexToPropertyDictionary.TryGetValue(index, out propDef))
                SetValue(propDef, destination, ParseValue(propDef, value));
        }

        public virtual TDest Parse(IList<string> values)
        {
            var destination = this._constructor();
            foreach (var item in this._indexToPropertyDictionary)
                SetValue(item.Value, destination, ParseValue(item.Value, values[item.Key]));
            return destination;
        }
    }
    //public static Func<IList<string>, T> StringsToObjectMappers<T>(ILineParserFactory<T> config) where T : new()
    //{
    //    return config.GetLineParser();
    //}
}
