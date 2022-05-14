using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Paillave.Etl.Autofac;
using Paillave.Etl.Core;
using Paillave.Etl.ExecutionToolkit;
using Paillave.Pdf;

namespace Paillave.Etl.Samples
{
    class PdfVisitor : IPdfVisitor
    {
        public void ProcessHeader(List<string> section, int pageNumber)
        {
        }

        public void ProcessLine(string text, int pageNumber, int lineNumber, int lineNumberInParagraph, int lineNumberInPage, List<string> section, string areaCode)
        {
            Console.WriteLine("-----------------------------------------------------------------------");
            Console.WriteLine(text);
        }

        public void ProcessTable(List<List<List<string>>> table, int pageNumber, List<string> section)
        {
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // var containerBuilder = new ContainerBuilder();
            // containerBuilder.RegisterInstance(new PdfVisitor()).AsImplementedInterfaces();
            // var container = containerBuilder.Build();
            // var etlResolver = new AutofacDependencyResolver(container);
            // etlResolver.TryResolve<IPdfVisitor>(out var tmp);
            // etlResolver.TryResolve<IPdfVisitor>(out tmp);


            // var processRunner = StreamProcessRunner.Create<string>(Import);
            // // ITraceReporter traceReporter = new AdvancedConsoleExecutionDisplay();
            // // traceReporter.Initialize(processRunner.GetDefinitionStructure());

            // // var tmp = processRunner.GetDefinitionStructure();
            // var res = await processRunner.ExecuteAsync("a", new ExecutionOptions<string>
            // {
            //     UseDetailedTraces = true,
            //     // TraceProcessDefinition = traceReporter.TraceProcessDefinition
            //     // TraceProcessDefinition = (teStream, cStream) =>
            //     // {
            //     //     // teStream.Do("trace", i => Console.WriteLine(i));
            //     // }
            // });

            using (var stream = File.OpenRead("Monument assurance Steuerbescheide2019-Searchable.pdf"))
            {
                var pdfReader = new PdfReader(stream, null, null, ExtractMethod.SimpleLines());
                pdfReader.Read(new PdfVisitor());
            }
        }
        // public static void Import(ISingleStream<string> contextStream)
        // {
        //     contextStream
        //         .CrossApply("ca", i => Enumerable.Range(0, 5).Select(j => $"{i}-{j}"))
        //         .SubProcess("sub", i => i.Do("show on screen", i => Console.WriteLine($"length: {i}")));
        //     // contextStream
        //     //     .SubProcess("sub process", stringStream => stringStream
        //     //         .Select("get string length", str => str.Length))
        //     //     .Do("show on screen", i => Console.WriteLine($"length: {i}"));
        //     // contextStream.SubProcess("sub process", stringStream => stringStream);
        //     // var stream1 = contextStream
        //     //     .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100)
        //     //         .Select(i => new { Id = i, Label = $"Label{i}" }));
        //     // var stream2 = contextStream
        //     //     .CrossApply("create values from enumeration2", ctx => Enumerable.Range(1, 8)
        //     //         .Select(i => new { Id = i, Label = $"OtherLabel{i}" }));
        //     // var res = stream1.Substract("merge with stream 2", stream2, i => i.Id, i => i.Id)
        //     //    .Do("print console", i => Console.WriteLine(i.Label));
        // }
    }
}
