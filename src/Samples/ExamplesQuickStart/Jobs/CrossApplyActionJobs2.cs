using Paillave.Etl;
using Paillave.Etl.Extensions;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.StreamNodes;
using System.Linq;

namespace ExamplesQuickStart.Jobs
{
    public class CrossApplyActionJobs2 : IStreamProcessDefinition<object>
    {
        public string Name => "import file";
        public void DefineProcess(ISingleStream<object> rootStream)
        {
            rootStream
                .CrossApplyEnumerable("create some values", (input) => Enumerable.Range(0, 10).Select(i => new MyInputType { Id = i, Value = (i % 3 == 0) ? i : (int?)null }))
                .Select("set null value to the previous not null value", new MySelectProcessor());
        }
        private class MyInputType
        {
            public int Id { get; set; }
            public int? Value { get; set; }
        }
        private class MyOutputType
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
        private class MySelectProcessor : ISelectProcessor<MyInputType, MyOutputType>
        {
            private int _lastValue = 0;
            public MyOutputType ProcessRow(MyInputType value)
            {
                if (value.Value != null) _lastValue = value.Value.Value;
                return new MyOutputType
                {
                    Id = value.Id,
                    Value = _lastValue
                };
            }
        }
    }
}
