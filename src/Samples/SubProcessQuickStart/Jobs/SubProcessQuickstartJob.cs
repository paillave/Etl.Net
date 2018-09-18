using SubProcessQuickStart.StreamTypes;
using System.IO;
using Paillave.Etl;
using Paillave.Etl.Core.Streams;
using System;

namespace SubProcessQuickStart.Jobs
{
    public class SubProcessQuickstartJob : IStreamProcessDefinition<MyConfig>
    {
        public string Name => "import file";

        public void DefineProcess(IStream<MyConfig> rootStream)
        {
            var outputFileResourceS = rootStream.Select("open output file", i => File.OpenWrite(i.DestinationFilePath));

            var parsedLineS = rootStream
                .CrossApplyFolderFiles("get folder files", i => i.InputFolderPath, i => i.InputFilesSearchPattern)
                .CrossApplyTextFile("parse input file", new InputFileRowMapper(), (i, p) => { p.FileName = i; return p; });

            var parsedTypeLineS = rootStream
                .Select("get input file type path", i => i.TypeFilePath)
                .CrossApplyTextFile("parse type input file", new TypeFileRowMapper());

            var categoryS = parsedTypeLineS.Distinct("get list of categories", i => i.Category);

            var joinedLineS = parsedLineS
                .Lookup("join types to file", parsedTypeLineS, i => i.TypeId, i => i.Id, (l, r) => new { l.Id, r.Name, l.FileName, r.Category });

            categoryS.ToSubProcesses("export data per category", singleCategoryS =>
            {
                var subProcessLines = joinedLineS
                    .Select("link to subprocess", singleCategoryS, (l, r) => new { Category = r.Category, Line = l })
                    .Where("keep only line for current sub process", i => i.Category == i.Line.Category)
                    .Select("keep line only", i => i.Line);

                var outputCategoryResourceS = singleCategoryS.Select("open output category file", rootStream, (i, j) => File.OpenWrite(Path.Combine(j.CategoryDestinationFolder, "Category-" + i.Category) + ".csv"));

                return subProcessLines
                    .Pivot("create statistic for categories", i => i.Category, i => new { Count = AggregationOperators.Count(), Total = AggregationOperators.Sum(i.Id) })
                    .Select("create output category data", i => new OutputCategoryRow { Category = i.Key, AmountOfEntries = i.Aggregation.Count, TotalAmount = i.Aggregation.Total })
                    .ToTextFile("write category statistics to file", outputCategoryResourceS, new OutputCategoryRowMapper());
            });

            joinedLineS.Select("create output data", i => new OutputFileRow { Id = i.Id, Name = i.Name, FileName = i.FileName })
                .ToTextFile("write to output file", outputFileResourceS, new OutputFileRowMapper())
                .ToAction("write to console", i => Console.WriteLine($"{i.FileName}:{i.Id}-{i.Name}"));
        }
    }
}
