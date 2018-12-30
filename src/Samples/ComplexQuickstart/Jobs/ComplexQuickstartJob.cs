using ComplexQuickstart.StreamTypes;
using System.IO;
using Paillave.Etl;
using Paillave.Etl.Extensions;
using Paillave.Etl.TextFile;
using Paillave.Etl.TextFile.Extensions;
using Paillave.Etl.Core.Streams;
using System;
using System.Linq;

namespace ComplexQuickstart.Jobs
{
    public class ComplexQuickstartJob
    {
        public static void DefineProcess(ISingleStream<MyConfig> rootStream)
        {
            var outputFileResourceS = rootStream.Select("open output file", i => (Stream)File.OpenWrite(i.DestinationFilePath));
            var outputCategoryResourceS = rootStream.Select("open output category file", i => (Stream)File.OpenWrite(i.CategoryDestinationFilePath));

            var parsedLineS = rootStream
                .CrossApplyFolderFiles("get folder files", i => i.InputFolderPath, i => i.InputFilesSearchPattern, (f, r) => f.Name)
                .CrossApplyTextFile("parse input file", new InputFileRowMapper(), (i, p) => { p.FileName = i; return p; });

            var parsedTypeLineS = rootStream
                .Select("get input file type path", i => i.TypeFilePath)
                .CrossApplyTextFile("parse type input file", new TypeFileRowMapper());

            var joinedLineS = parsedLineS
                .Lookup("join types to file", parsedTypeLineS, i => i.TypeId, i => i.Id, (l, r) => new { l.Id, r.Name, l.FileName, r.Category });

            var categoryStatistics = joinedLineS
                .Pivot("create statistic for categories", i => i.Category, i => new { Count = AggregationOperators.Count(), Total = AggregationOperators.Sum(i.Id) })
                .Select("create output category data", i => new OutputCategoryRow { Category = i.Key, AmountOfEntries = i.Aggregation.Count, TotalAmount = i.Aggregation.Total })
                .ThroughTextFile("write category statistics to file", outputCategoryResourceS, new OutputCategoryRowMapper());

            joinedLineS.Select("create output data", i => new OutputFileRow { Id = i.Id, Name = i.Name, FileName = i.FileName })
                .ThroughTextFile("write to output file", outputFileResourceS, new OutputFileRowMapper())
                .ThroughAction("write to console", i => Console.WriteLine($"{i.FileName}:{i.Id}-{i.Name}"));
        }
        public static void DefineSimpleProcess(ISingleStream<MySimpleConfig> rootStream)
        {
            var parsedLineS = rootStream
                .CrossApplyTextFile("parse input file", new InputFileRowMapper(), i => i.InputFilesPath);

            var parsedTypeLineS = rootStream
                .CrossApplyTextFile("parse type input file", new TypeFileRowMapper(), i => i.TypeFilePath);

            var joinedLineS = parsedLineS
                .Lookup("join types to file", parsedTypeLineS, i => i.TypeId, i => i.Id, (l, r) => new { l.Id, r.Name, l.FileName, r.Category });
        }
        public static void DefineProcessWithError(ISingleStream<MySimpleConfig> rootStream)
        {
            var parsedLineS = rootStream
                .CrossApplyEnumerable("input file", c =>
                {
                    var lst = Enumerable.Range(1, 100).Select(i => new InputFileRow { Id = i }).ToList();
                    return lst;
                });

            var parsedTypeLineS = rootStream
                .CrossApply<MySimpleConfig, TypeFileRow>("type file", (c, push) =>
                {
                    Console.WriteLine($"raising exception {Guid.NewGuid()}");
                    throw new Exception();
                }, true);

            parsedLineS
                .Lookup("join types to file", parsedTypeLineS, i => i.TypeId, i => i.Id, (l, r) => new { l.Id, r.Name, l.FileName, r.Category });
        }
    }
}
