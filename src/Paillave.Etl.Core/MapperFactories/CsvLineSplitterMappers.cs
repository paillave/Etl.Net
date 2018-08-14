using Paillave.Etl.Core.Helpers.MapperFactories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.MapperFactories
{
    public static partial class Mappers
    {
        public static Func<string, IList<string>> CsvLineSplitter(char fieldDemimiter = ';', char textDemimiter = '"')
        {
            return new CsvSplit(fieldDemimiter, textDemimiter).ParseCsvLine;
        }
        public static Func<IList<string>, string> CsvLineJoiner(char fieldDemimiter = ';', char textDemimiter = '"')
        {
            return new CsvSplit(fieldDemimiter, textDemimiter).JoinCsvLine;
        }
    }
}
