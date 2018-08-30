using ConsoleApp1.StreamTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Paillave.Etl;
using Paillave.Etl.Core.Streams;

namespace ConsoleApp1.Jobs
{
    public class TestJob6 : IStreamProcessDefinition<MyConfig>
    {
        public string Name => "import file";

        public void DefineProcess(IStream<MyConfig> rootStream)
        {
            rootStream
                .Select("get input file type path", i => i.TypeFilePath)
                .CrossApplyTextFile("parse type input file", new TypeFileRowMapper())
                .Select("select outpout", i => $"{i.Id}-{i.Name}-{i.Category}")
                .ToAction("to console", Console.WriteLine);
        }
    }
}
