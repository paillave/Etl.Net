using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.Core.Helpers.MapperFactories
{
    public class ColumnNameMappingConfiguration<TDest>
    {
        public ColumnNameMappingConfiguration(Func<TDest> constructor)
        {
            this._constructor = constructor;
        }
        private IDictionary<string, PropertyDescription> _columnDictionary = new Dictionary<string, PropertyDescription>(StringComparer.OrdinalIgnoreCase);
        private CultureInfo _cultureInfo = null;
        private readonly Func<TDest> _constructor;

        public ColumnNameMappingConfiguration<TDest> MapColumnToProperty<TField>(string columnName, Expression<Func<TDest, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            this._columnDictionary[columnName] = new PropertyDescription { MemberLamda = memberLamda, CultureInfo = cultureInfo };
            return this;
        }
        public ColumnNameMappingConfiguration<TDest> WithCultureInfo(CultureInfo cultureInfo)
        {
            this._cultureInfo = cultureInfo;
            return this;
        }

        public Func<IList<string>, TDest> LineParser(IList<string> columnNameIndex)
        {
            return new LineParser<TDest>(columnNameIndex
                .Select((ColumnName, Index) => new { ColumnName, Index })
                .Join(
                    _columnDictionary,
                    i => i.ColumnName,
                    i => i.Key,
                    (l, r) =>
                    {
                        var propertyDescription = new PropertyDescription
                        {
                            CultureInfo = r.Value.CultureInfo ?? this._cultureInfo,
                            MemberLamda = r.Value.MemberLamda
                        };
                        return new
                        {
                            Index = l.Index,
                            PropertyMap = new PropertyMapper(propertyDescription)
                        };
                    },
                    StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(i => i.Index, i => i.PropertyMap),
                    this._constructor).Parse;
        }
    }
}
