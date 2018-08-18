using ConsoleApp1.StreamTypes;
using Paillave.Etl;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConsoleApp1.Jobs
{
    public class TestJob2 : ExecutionContext<MyConfig>
    {
        public TestJob2() : base("import file")
        {
            var outputFileResourceS = StartupStream.Select("open output file", i => new StreamWriter(i.DestinationFilePath));

            var parsedTypeLineS = StartupStream
                .Select("get input file type path", i => i.TypeFilePath)
                .CrossApplyTextFile("parse type input file", new TypeFileRowMapper())
                .Select("output after join", i => new OutputFileRow { FileName = "aze", Id = i.Id, Name = i.Name })
                .ToTextFile("write copy", outputFileResourceS, new OutputFileRowMapper())
                .Select("create text to console", i => $"{i.Id}-{i.Name}")
                .ToAction("write to console", Console.WriteLine);
        }
    }
}
