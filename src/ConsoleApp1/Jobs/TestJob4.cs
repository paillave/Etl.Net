using ConsoleApp1.StreamTypes;
using Paillave.Etl;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Paillave.Etl.Core.Streams;
using System.Linq;

namespace ConsoleApp1.Jobs
{
    public class TestJob4 : IStreamProcessDefinition<MyConfig>
    {
        public string Name => "import file";

        public void DefineProcess(IStream<MyConfig> rootStream)
        {
            var outputFileResourceS = rootStream
                .CrossApplyAction<MyConfig, int>("produce values", (config, pushValue) =>
                {
                    for (int i = -10; i < 10; i++)
                        pushValue(i);
                });
            outputFileResourceS
                .Select("computation", i => new { Compute = 1 / i })
                .Select("render console text", i => $"{i.Compute}")
                .ToAction("show to console", Console.WriteLine);
        }
    }
}
