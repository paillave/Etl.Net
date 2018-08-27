using System;
using Paillave.Etl.Helpers.MapperFactories;
using System.Globalization;
using System.Linq.Expressions;

namespace Paillave.Etl.Helpers
{
    public class ColumnNameFlatFileDescriptor<T> : FlatFileDescriptorBase<T> where T : new()
    {
        internal ColumnNameMappingConfiguration<T> ColumnNameMappingConfiguration { get; private set; }

        public ColumnNameFlatFileDescriptor()
        {
            this.ColumnNameMappingConfiguration = new ColumnNameMappingConfiguration<T>(() => new T());
        }
        public void MapColumnToProperty<TField>(string columnName, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            this.ColumnNameMappingConfiguration.MapColumnToProperty(columnName, memberLamda, cultureInfo);
        }
        public void WithCultureInfo(CultureInfo cultureInfo)
        {
            this.ColumnNameMappingConfiguration.WithCultureInfo(cultureInfo);
        }
    }
}