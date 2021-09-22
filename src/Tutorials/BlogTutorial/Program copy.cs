using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace BlogTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess7);
            var executionOptions = new ExecutionOptions<string>
            {
                // TraceProcessDefinition = DefineTraceProcess
            };
            var res = await processRunner.ExecuteAsync(args[0], executionOptions);
            // var res2 = await processRunner.ExecuteAsync(args[0], executionOptions);
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
        private static void DefineProcess71(ISingleStream<string> contextStream)
        {
            // var tmp = 0;
            // contextStream
            //      .Select("zzer", i => 1 / tmp);
            contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable
                .Range(1, 100)
                .Select(i => new { Id = i, OutputId = i % 10, label = $"Label{i}" }));


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
                    .Select(i => new { Id = 1, label = $"OtherLabel{i}" }));

            stream1.Lookup("join output values", streamToLookup, l => l.OutputId, r => r.Id, (l, r) => new { FromLeft = l, FromRight = r });
        }
        private static void DefineProcess100(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100)
                    .Select(i => new
                    {
                        OutputId = i % 11,
                        Label = $"{ctx}{i}",
                        Description = (i % 5 == 0) ? null : $"Description {i}"
                    }))
                .GroupBy("group by OutputId", i => i.OutputId)
                .Do("print file name to console", i => Console.WriteLine($"{i.Key}: {i.Aggregation.Count} items"));
        }
        private static void DefineProcess101(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100)
                    .Select(i => new
                    {
                        Id = i,
                        OutputId = i % 11,
                        Label = $"{ctx}{i}",
                        Description = (i % 5 == 0) ? null : $"Description {i}"
                    }))
                .Aggregate("aggregate by OutputId",
                    i => i.OutputId,
                    i => new { Key = i.OutputId, Ids = new List<int>() },
                    (a, v) =>
                    {
                        a.Ids.Add(v.Id);
                        return a;
                    })
                .Do("print file name to console", i => Console.WriteLine($"{i.Key}: {i.Aggregation.Ids.Count} items"));
        }
        private static void DefineProcess102(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100)
                    .Select(i => new
                    {
                        Id = i,
                        OutputId = i % 3,
                        Label = $"{ctx}{i}",
                        Description = (i % 5 == 0) ? null : $"Description {i}"
                    }))
                .Pivot("pivot values", i => i.OutputId, i => new
                {
                    Count = AggregationOperators.Count(),
                    Count0 = AggregationOperators.Count().For(i.OutputId == 0),
                    Count1 = AggregationOperators.Count().For(i.OutputId == 1),
                    Count2 = AggregationOperators.Count().For(i.OutputId == 2)
                })
                .Do("print file name to console", i => Console.WriteLine($"{i.Key}: Count={i.Aggregation.Count}, Count0={i.Aggregation.Count0}, Count1={i.Aggregation.Count1}, Count2={i.Aggregation.Count2}"));
        }
        private static void DefineProcess103(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100))
                .Select("Create a value", i => new
                {
                    Id = i,
                    OutputId = i % 3,
                    Label = $"Label{i}",
                })
                .Do("print file name to console", i => Console.WriteLine($"{i.Id}-{i.Label}"));
        }
        private static void DefineProcess106(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100))
                .Select("Create a value", i => new
                {
                    Id = i,
                    OutputId = i % 3,
                    Label = $"Label{i}",
                })
                .Do("print file name to console", i => Console.WriteLine($"{i.Id}-{i.Label}"));
        }
        class TempoContext
        {
            public int Value1 { get; set; }
            public string Value2 { get; set; }
        }
        private static void DefineProcess104(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100))
                .Select("Create a value", new ValueProcessorWithContext<int, string, TempoContext>(
                    new TempoContext(),
                    (int v, TempoContext ctx) =>
                    {
                        if (v == 12)
                            ctx.Value1++;
                        if (v == 5)
                            ctx.Value2 = $"5 value already passed";
                        return $"{ctx.Value1}-{v} {ctx.Value2}";
                    }))
                .Do("print file name to console", Console.WriteLine);
        }
        private static void DefineProcess105(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100))
                .Select("Create a value", i => new
                {
                    Id = i,
                    OutputId = i % 3,
                    Label = (i % 3 == 2) ? null : $"Label{i}"
                })
                .Fix("fix value", o => o
                    .FixProperty(i => i.OutputId).AlwaysWith(i => i.OutputId * 10)
                    .FixProperty(i => i.Label).IfNullWith(i => $"New Label {i.Id}"))
                .Do("print file name to console", i => Console.WriteLine($"{i.Id}-{i.Label}"));
        }

        class ValueProcessorWithContext<TIn, TOut, TCtx> : ISelectProcessor<TIn, TOut>
        {
            private readonly TCtx _context;
            private readonly Func<TIn, TCtx, TOut> _process;
            public ValueProcessorWithContext(TCtx context, Func<TIn, TCtx, TOut> process) => (_context, _process) = (context, process);
            public TOut ProcessRow(TIn value) => _process(value, _context);
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
