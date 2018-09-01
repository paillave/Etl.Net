using Paillave.Etl;
using System.IO;
using Paillave.Etl.Helpers;
using Paillave.Etl.Core.Streams;
using System;

namespace ConsoleApp1
{
    public class SimpleConfig
    {
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
    }

    public class SimpleInputFileRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CategoryCode { get; set; }
    }

    public class SimpleInputFileRowMapper : ColumnNameFlatFileDescriptor<SimpleInputFileRow>
    {
        public SimpleInputFileRowMapper()
        {
            this.MapColumnToProperty("#", i => i.Id);
            this.MapColumnToProperty("Label", i => i.Name);
            this.MapColumnToProperty("Category", i => i.CategoryCode);
            this.IsFieldDelimited('\t');
        }
    }

    public class CategoryStatisticFileRow
    {
        public string CategoryCode { get; set; }
        public int Count { get; set; }
    }

    public class CategoryStatisticFileRowMapper : ColumnNameFlatFileDescriptor<CategoryStatisticFileRow>
    {
        public CategoryStatisticFileRowMapper()
        {
            this.MapColumnToProperty("Category", i => i.CategoryCode);
            this.MapColumnToProperty("Nb", i => i.Count);
        }
    }

    public class SimpleQuickstartJob : IStreamProcessDefinition<SimpleConfig>
    {
        public string Name => "Simple quickstart";

        public void DefineProcess(IStream<SimpleConfig> rootStream)
        {
            var outputFileS = rootStream.Select("open output file", i => new StreamWriter(i.OutputFilePath));
            rootStream
                .CrossApplyTextFile("read input file", new SimpleInputFileRowMapper(), i => i.InputFilePath)
                .ToAction("Write input file to console", i => Console.WriteLine($"{i.Id}-{i.Name}-{i.CategoryCode}"))
                .Pivot("group and count", i => i.CategoryCode, i => new { Count = AggregationOperators.Count() })
                .Select("create output row", i => new CategoryStatisticFileRow { CategoryCode = i.Key, Count = i.Aggregation.Count })
                .Sort("sort output values", i => new { i.CategoryCode })
                .ToTextFile("write to text file", outputFileS, new CategoryStatisticFileRowMapper());
        }
    }

    class ProgramOld
    {
        static void MainOld(string[] args)
        {
            new StreamProcessRunner<SimpleQuickstartJob, SimpleConfig>().ExecuteAsync(new SimpleConfig
            {
                InputFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\simpleinputfile.csv",
                OutputFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\simpleoutputfile.csv"
            }, null).Wait();
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
