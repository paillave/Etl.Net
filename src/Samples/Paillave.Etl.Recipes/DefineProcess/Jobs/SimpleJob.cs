using Paillave.Etl.Extensions;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Recipes.DefineProcess.StreamTypes.Config;

namespace Paillave.Etl.Recipes.DefineProcess.Jobs
{
    public class SimpleJob
    {
        public static void Job1(ISingleStream<SimpleConfigStreamType> config)
        {
            config.ThroughAction("show message", i => i.Messages.Add($"{100 / i.Divider} times hello world!"));
        }
    }
}