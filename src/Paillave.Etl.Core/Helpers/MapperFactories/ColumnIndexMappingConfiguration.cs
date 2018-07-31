using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.Core.Helpers.MapperFactories
{
    public class ColumnIndexMappingConfiguration<TDest>
    {
        public ColumnIndexMappingConfiguration(Func<TDest> constructor)
        {
            this._constructor = constructor;
        }
        private IDictionary<int, PropertyDescription> _columnDictionary = new Dictionary<int, PropertyDescription>();
        private CultureInfo _cultureInfo = null;
        private readonly Func<TDest> _constructor;

        public ColumnIndexMappingConfiguration<TDest> MapColumnToProperty<TField>(int index, Expression<Func<TDest, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            this._columnDictionary[index] = new PropertyDescription { MemberLamda = memberLamda, CultureInfo = cultureInfo };
            return this;
        }
        public ColumnIndexMappingConfiguration<TDest> WithCultureInfo(CultureInfo cultureInfo)
        {
            this._cultureInfo = cultureInfo;
            return this;
        }

        public Func<IList<string>, TDest> LineParser()
        {
            return new LineParser<TDest>(_columnDictionary.ToDictionary(i => i.Key, r =>
            {
                var propertyDescription = new PropertyDescription
                {
                    CultureInfo = r.Value.CultureInfo ?? this._cultureInfo,
                    MemberLamda = r.Value.MemberLamda
                };
                return new PropertyMapper(propertyDescription);
            }),
            this._constructor).Parse;
        }
    }
}
