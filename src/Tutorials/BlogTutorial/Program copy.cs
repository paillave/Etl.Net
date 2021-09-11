using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace BlogTutorial
{
    class Program2
    {
        static async Task Main2(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            var executionOptions = new ExecutionOptions<string>
            {
                TraceProcessDefinition = DefineTraceProcess
            };
            var res = await processRunner.ExecuteAsync(args[0], executionOptions);
        }
        private static void DefineTraceProcess(IStream<TraceEvent> traceStream, ISingleStream<string> contentStream)
        {
            // TODO: define how to process traces here
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100).Select(i => $"{ctx}{i}"))
                .Do("print file name to console", i => Console.WriteLine(i));
        }
        private static void DefineProcess3(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100))
                .Select("apply single stream content", contextStream, (l, r) => $"{l}-{r}");
        }
        private static void DefineProcess6(ISingleStream<string> contextStream)
        {
            var sortedStream1 = contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable
                    .Range(1, 100)
                    .Select(i => new { Id = i, OutputId = i % 10, label = $"Label{i}" }))
                .EnsureSorted("ensure it is sorted on OutputId", i => new { i.OutputId });

            var streamToLookup = contextStream
                .CrossApply("create values from enumeration2", ctx => Enumerable
                    .Range(1, 8)
                    .Select(i => new { Id = i, label = $"OtherLabel{i}" }))
                .EnsureKeyed("ensure it is keyed on Id", i => new { OutputId = i.Id });

            sortedStream1.LeftJoin("join output values", streamToLookup, (l, r) => new { FromLeft = l, FromRight = r });
        }
        private static void DefineProcess8(ISingleStream<string> contextStream)
        {
            var stream1 = contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable
                    .Range(1, 100)
                    .Select(i => new { Id = i, label = $"Label{i}" }));
            var stream2 = contextStream
                .CrossApply("create values from enumeration2", ctx => Enumerable
                    .Range(1, 8)
                    .Select(i => new { Id = i, label = $"OtherLabel{i}" }));

            var res = stream1.Union("merge with stream 2", stream2);
        }
        class Tmp1
        {
            public int Id { get; set; }
            public string Label { get; set; }
        }
        private static void DefineProcess9(ISingleStream<string> contextStream)
        {
            var stream1 = contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable
                    .Range(1, 100)
                    .Select(i => new Tmp1 { Id = i, Label = $"Label{i}" }));
            var stream2 = contextStream
                .CrossApply("create values from enumeration2", ctx => Enumerable
                    .Range(1, 8)
                    .Select(i => new Tmp1 { Id = i, Label = $"OtherLabel{i}" }));

            var res = stream1.Substract("merge with stream 2", stream2, i => i.Id, i => i.Id);
        }
        private static void DefineProcess10(ISingleStream<string> contextStream)
        {
            var stream1 = contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable
                    .Range(1, 100)
                    .Select(i => new Tmp1 { Id = i, Label = $"Label{i}" }))
                .EnsureSorted("ensure it is sorted on Id", i => new { i.Id });
            var stream2 = contextStream
                .CrossApply("create values from enumeration2", ctx => Enumerable
                    .Range(1, 8)
                    .Select(i => new Tmp1 { Id = i, Label = $"OtherLabel{i}" }))
                .EnsureKeyed("ensure it is keyed on Id2", i => new { i.Id });

            var res = stream1.Substract("merge with stream 2", stream2);
        }
        private static void DefineProcess7(ISingleStream<string> contextStream)
        {
            var stream1 = contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable
                    .Range(1, 100)
                    .Select(i => new { Id = i, OutputId = i % 10, label = $"Label{i}" }));

            var streamToLookup = contextStream
                .CrossApply("create values from enumeration2", ctx => Enumerable
                    .Range(1, 8)
                    .Select(i => new { Id = i, label = $"OtherLabel{i}" }));

            stream1.Lookup("join output values", streamToLookup, l => l.OutputId, r => r.Id, (l, r) => new { FromLeft = l, FromRight = r });
        }
        private static void DefineProcess4(ISingleStream<string> contextStream)
        {
            contextStream.CrossApply("create values from factory", SimpleValuesProvider.Create<string, string>((ctx, dependencyResolver, cancellationToken, push) =>
                {
                    for (int i = 1; i <= 100; i++)
                        push($"{ctx}{i}");
                }));
        }
        private static void DefineProcess2(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApply<string, string>("create values from factory", (ctx, dependencyResolver, cancellationToken, push) =>
                {
                    for (int i = 1; i <= 100; i++)
                        push($"{ctx}{i}");
                })
                .Do("print file name to console", i => Console.WriteLine(i));
        }
        private static void DefineProcess5(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApply<string, string>("create values from factory", new DemoValueProvider())
                .Do("print file name to console", i => Console.WriteLine(i));
        }
        public class DemoValueProvider : IValuesProvider<string, string>
        {
            public string TypeName => "Range of values provider";
            public ProcessImpact PerformanceImpact => ProcessImpact.Light;
            public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
            public void PushValues(string input, Action<string> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
            {
                for (int i = 1; i <= 100; i++)
                    push($"{input}{i}");
            }
        }
    }
}
