using System;
using Paillave.Etl.Core.Helpers.MapperFactories;
using System.Globalization;
using System.Linq.Expressions;

namespace Paillave.Etl.Core.Helpers
{
    public class ColumnIndexFlatFileDescriptor<T> : FlatFileDescriptorBase<T> where T : new()
    {
        internal ColumnIndexMappingConfiguration<T> ColumnIndexMappingConfiguration { get; private set; }

        public ColumnIndexFlatFileDescriptor()
        {
            this.ColumnIndexMappingConfiguration = new ColumnIndexMappingConfiguration<T>(() => new T());
        }
        public void MapColumnToProperty<TField>(int index, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            this.ColumnIndexMappingConfiguration.MapColumnToProperty(index, memberLamda, cultureInfo);
        }
        public void WithCultureInfo(CultureInfo cultureInfo)
        {
            this.ColumnIndexMappingConfiguration.WithCultureInfo(cultureInfo);
        }
    }
}