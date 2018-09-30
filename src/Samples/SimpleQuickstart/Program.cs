using Paillave.Etl;
using System;
using System.IO;
using Paillave.Etl.Core;
using Paillave.Etl.TextFile.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.TextFile;

namespace SimpleQuickstart
{
    public class SimpleConfig
    {
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
    }

    public class SimpleQuickstartJob : IStreamProcessDefinition<SimpleConfig>
    {
        public string Name => "Simple quickstart";

        public void DefineProcess(IStream<SimpleConfig> rootStream)
        {
            var outputFileS = rootStream.Select("open output file", i => File.OpenWrite(i.OutputFilePath));
            rootStream
                .CrossApplyTextFile("read input file",
                    FlatFileDefinition.Create(i => new
                    {
                        Id = i.ToColumn<int>("#"),
                        Name = i.ToColumn<string>("Label"),
                        ValueType = i.ToColumn<string>("Value"),
                        CategoryCode = i.ToColumn<string>("Category")
                    }).IsColumnSeparated('\t'),
                    i => i.InputFilePath)
                .ThroughAction("Write input file to console", 
                    i => Console.WriteLine($"{i.Id}->{i.Name}->{i.CategoryCode}->{i.ValueType}"))
                .Pivot("group and count", 
                    i => i.CategoryCode, 
                    i => new
                    {
                        Count = AggregationOperators.Count(),
                        CountA = AggregationOperators.Count().For(i.ValueType == "a"),
                        CountB = AggregationOperators.Count().For(i.ValueType == "b"),
                    })
                .Select("create output row",
                    i => new
                    {
                        CategoryCode = i.Key,
                        i.Aggregation.Count,
                        i.Aggregation.CountA,
                        i.Aggregation.CountB
                    })
                .Sort("sort output values", 
                    i => new { i.CategoryCode })
                .ThroughTextFile("write to text file",
                    outputFileS,
                    FlatFileDefinition.Create(i => new
                    {
                        CategoryCode = i.ToColumn<string>("MyCategoryCode"),
                        Count = i.ToColumn<int>("Count"),
                        CountA = i.ToColumn<int>("CountA"),
                        CountB = i.ToColumn<int>("CountB")
                    }));
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var testFilesDirectory = args[0];

            new StreamProcessRunner<SimpleQuickstartJob, SimpleConfig>().ExecuteAsync(new SimpleConfig
            {
                InputFilePath = Path.Combine(testFilesDirectory, "simpleinputfile.csv"),
                OutputFilePath = Path.Combine(testFilesDirectory, "simpleoutputfile.csv")
            }, null).Wait();
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
