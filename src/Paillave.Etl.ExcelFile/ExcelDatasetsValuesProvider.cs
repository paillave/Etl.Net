using Paillave.Etl.Core;
using System;
using System.Linq;
using System.Threading;
using ExcelDataReader;
using System.Data;
using System.Collections.Generic;

namespace Paillave.Etl.ExcelFile
{
    public class ExcelDatasetsValuesProviderArgs<TOut>
    {
        public Func<DataTable, IFileValue, IEnumerable<TOut>?> GetOutput { get; set; }
        public bool UseStreamCopy { get; set; } = true;
    }
    public class ExcelDatasetsValuesProvider<TOut> : ValuesProviderBase<IFileValue, TOut>
    {
        private ExcelDatasetsValuesProviderArgs<TOut> _args;
        public ExcelDatasetsValuesProvider(ExcelDatasetsValuesProviderArgs<TOut> args) => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        public override void PushValues(IFileValue input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            using var stream = input.Get(_args.UseStreamCopy);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataset = reader.AsDataSet();
            dataset.DataSetName = input.Name;
            foreach (var item in dataset.Tables.Cast<DataTable>())
            {
                var ret = _args.GetOutput(item, input);
                if (ret != null)
                    ret.ToList().ForEach(push);
            }
        }
    }

    public class ExcelDataTablesValuesProviderArgs
    {
        public bool UseStreamCopy { get; set; } = true;
    }
    public class ExcelDataTablesValuesProvider : ValuesProviderBase<IFileValue, DataTable>
    {
        private readonly ExcelDataTablesValuesProviderArgs _args;

        public ExcelDataTablesValuesProvider(ExcelDataTablesValuesProviderArgs? args = null)
            => (_args) = (args ?? new());

        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        public override void PushValues(IFileValue input, Action<DataTable> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            using var stream = input.Get(_args.UseStreamCopy);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataset = reader.AsDataSet();
            dataset.DataSetName = input.Name;
            dataset.Tables.Cast<DataTable>().ToList().ForEach(push);
        }
    }
}
