using ConsoleApp1.StreamTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Paillave.Etl;
using Paillave.Etl.Core.Streams;

namespace ConsoleApp1.Jobs
{
    public class TestJob3 : IStreamProcessDefinition<MyConfig>
    {
        public string Name => "import file";

        public void DefineProcess(IStream<MyConfig> rootStream)
        {
            var outputFileResourceS = rootStream.Select("open output file", i => new StreamWriter(i.DestinationFilePath));
            var outputCategoryResourceS = rootStream.Select("open output category file", i => new StreamWriter(i.CategoryDestinationFilePath));

            var parsedLineS = rootStream
                .CrossApplyFolderFiles("get folder files", i => i.InputFolderPath, i => i.InputFilesSearchPattern)
                .CrossApplyTextFile("parse input file", new InputFileRowMapper(), (i, p) => { p.FileName = i; return p; });

            var parsedTypeLineS = rootStream
                .Select("get input file type path", i => i.TypeFilePath)
                .CrossApplyTextFile("parse type input file", new TypeFileRowMapper());

            var joinedLineS = parsedLineS
                .Lookup("join types to file", parsedTypeLineS, i => i.TypeId, i => i.Id, (l, r) => new { l.Id, r.Name, l.FileName, r.Category });

            var categoryStatistics = joinedLineS.Aggregate("create statistic for categories", (i) => 0, i => i.Category, (a, v) => a + 1, (i, k, a) => new OutputCategoryRow { Category = k, AmountOfEntries = a })
                .ToTextFile("write category statistics to file", outputCategoryResourceS, new OutputCategoryRowMapper());

            joinedLineS.Select("create output data", i => new OutputFileRow { Id = i.Id, Name = i.Name, FileName = i.FileName })
                .ToTextFile("write to output file", outputFileResourceS, new OutputFileRowMapper());
            //.ToAction("write to console", i => Console.WriteLine($"{i.FileName}:{i.Id}-{i.Name}"));
        }
    }
}
