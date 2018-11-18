using SubProcessQuickStart.StreamTypes;
using System.IO;
using Paillave.Etl;
using Paillave.Etl.Extensions;
using Paillave.Etl.TextFile;
using Paillave.Etl.TextFile.Extensions;
using Paillave.Etl.Core.Streams;
using System;

namespace SubProcessQuickStart.Jobs
{
    public class SubProcessQuickstartJob
    {
        public static void DefineProcess(ISingleStream<MyConfig> rootStream)
        {
            var outputFileResourceS = rootStream.Select("open output file", i => (Stream)File.OpenWrite(i.DestinationFilePath));

            var parsedLineS = rootStream
                .CrossApplyFolderFiles("get folder files", i => i.InputFolderPath, i => i.InputFilesSearchPattern, (f, r) => f.Name)
                .CrossApplyTextFile("parse input file", new InputFileRowMapper(), (i, p) => { p.FileName = i; return p; });

            var parsedTypeLineS = rootStream
                .Select("get input file type path", i => i.TypeFilePath)
                .CrossApplyTextFile("parse type input file", new TypeFileRowMapper());

            var categoryS = parsedTypeLineS.Distinct("get list of categories", i => i.Category);

            var joinedLineS = parsedLineS
                .Lookup("join types to file", parsedTypeLineS, i => i.TypeId, i => i.Id, (l, r) => new { l.Id, r.Name, l.FileName, r.Category })
                .Select("join config and data", rootStream, (l, r) => new { Data = l, Cfg = r })
                .GroupBy("export data per category", i => i.Data.Category, groupedLines =>
                {
                    var groupOutputFileS = groupedLines
                        .Top("Take first row", 1)
                        .EnsureSingle("Ensure only one element in group")
                        .Select("Open output file", i => (Stream)File.OpenWrite(Path.Combine(i.Cfg.CategoryDestinationFolder, $"Category-{i.Data.Category}.csv")));
                    groupedLines
                        .Select("create output data", i => new OutputFileRow { Id = i.Data.Id, Name = i.Data.Name, FileName = i.Data.FileName })
                        .ThroughTextFile("Write lines to matching category text file", groupOutputFileS, new OutputFileRowMapper());
                    return groupedLines;
                });

            joinedLineS.Select("create output data", i => new OutputFileRow { Id = i.Data.Id, Name = i.Data.Name, FileName = i.Data.FileName })
                .ThroughTextFile("write to output one single file", outputFileResourceS, new OutputFileRowMapper())
                .ThroughAction("write to console", i => Console.WriteLine($"{i.FileName}:{i.Id}-{i.Name}"));
        }
    }
}
