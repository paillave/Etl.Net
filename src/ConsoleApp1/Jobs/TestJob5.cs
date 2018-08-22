using ConsoleApp1.StreamTypes;
using Paillave.Etl;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Paillave.Etl.Core.Streams;
using System.Linq;
using ConsoleApp1.Resources;

namespace ConsoleApp1.Jobs
{
    public class TestJob5 : IStreamProcessDefinition<MyConfig2>
    {
        public string Name => "import file";

        public void DefineProcess(IStream<MyConfig2> rootStream)
        {
            var dbContextS = rootStream.Select("create dbcontext", i => new TestDbContext(i.ConnectionString));
            var dbValueS = rootStream.CrossApplyEntityFrameworkCoreQuery("get values from db", dbContextS, (c, dbContext) => dbContext.Input.Where(i => i.Name.Contains(c.Filter)));
            dbValueS
                .Select("create output values", i => new MyOutputValue { Name = i.Name })
                .ToEntityFrameworkCore("save values to dbcontext", dbContextS)
                .Select("render console text", i => $"{i.Id}-{i.Name}")
                .ToAction("show to console", Console.WriteLine);
        }
    }
}
