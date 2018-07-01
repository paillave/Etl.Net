using Paillave.Etl.Core.Helpers.MapperFactories;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Paillave.Etl.Core.MapperFactories
{
    public static partial class Mappers
    {
        public static ColumnNameMappingConfiguration<TDest> ColumnNameStringParserMappers<TDest>() where TDest : new()
        {
            return new ColumnNameMappingConfiguration<TDest>(() => new TDest());
        }
        //public static ColumnNameMappingConfiguration<TDest> ColumnNameStringParserMappers<TDest>(Func<TDest> prototypeConstructor)
        //{
        //    return new ColumnNameMappingConfiguration<TDest>(prototypeConstructor);
        //}
        //public static Func<IList<string>, T> StringsToObjectMappers<T>(ILineParserFactory<T> config) where T : new()
        //{
        //    return config.GetLineParser();
        //}
    }
}
