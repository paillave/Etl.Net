using Paillave.Etl;
using Paillave.Etl.Core.Streams;
using System.Linq;

namespace ExamplesQuickStart.Jobs
{
    public class CrossApplyActionJobs : IStreamProcessDefinition<object>
    {
        public string Name => "import file";
        public void DefineProcess(IStream<object> rootStream)
        {
            rootStream.CrossApplyEnumerable("create some values", (input) => Enumerable.Range(0, 10));
        }
    }
}
