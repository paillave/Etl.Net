using Paillave.Etl.Core;
using System;
using System.Linq;
using System.Threading;
using ExcelDataReader;
using System.Data;

namespace Paillave.Etl.ExcelFile
{
    public class ExcelDatasetsValuesProviderArgs<TOut>
    {
        public Func<DataTable, IFileValue, TOut> GetOutput { get; set; }
    }
    public class ExcelDatasetsValuesProvider<TOut> : ValuesProviderBase<IFileValue, TOut>
    {
        private ExcelDatasetsValuesProviderArgs<TOut> _args;
        public ExcelDatasetsValuesProvider(ExcelDatasetsValuesProviderArgs<TOut> args) => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        public override void PushValues(IFileValue input, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            using (var reader = ExcelReaderFactory.CreateReader(input.GetContent()))
            {
                var dataset = reader.AsDataSet();
                dataset.DataSetName = input.Name;
                foreach (var item in dataset.Tables.Cast<DataTable>())
                    push(_args.GetOutput(item, input));
            }
        }
    }
}
