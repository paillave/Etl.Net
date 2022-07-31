using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Autofac;
using Paillave.Etl.Core;
using Paillave.Etl.EntityFrameworkCore;
using Paillave.Etl.ExecutionToolkit;
using Paillave.Pdf;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Filters;
using UglyToad.PdfPig.Graphics.Colors;

namespace Paillave.Etl.Samples
{
    class PdfVisitor : IPdfVisitor
    {
        public Dictionary<string, List<string>> Lines { get; } = new Dictionary<string, List<string>>();
        private List<string> GetLines(string area)
        {
            if (Lines.TryGetValue(area, out var lines)) return lines;
            lines = new List<string>();
            Lines[area] = lines;
            return lines;
        }
        public void ProcessHeader(List<string> section, int pageNumber)
        {
        }

        public void ProcessLine(string text, int pageNumber, int lineNumber, int lineNumberInParagraph, int lineNumberInPage, List<string> section, HashSet<string> areas)
        {
            foreach (var area in areas)
                GetLines(area).Add(text);
        }

        public void ProcessTable(List<List<List<string>>> table, int pageNumber, List<string> section)
        {
        }
    }
    class Program5
    {
        static void Main(string[] args)
        {
            // var builder = new DbContextOptionsBuilder<PmsDbContext>();
            // builder.UseSqlServer("Server=tcp:fundprocessprod.database.windows.net,1433;Initial Catalog=IREPreProd;Persist Security Info=False;User ID=ApplicationUser;Password=Pms.Application.User;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=300;");
            // var context = new PmsDbContext(builder.Options, new TenantContext(1));
            // var cur = context.Set<Currency>().First();
            // var secus = new List<Security> {
            //     new FxForward
            //     {
            //         InternalCode="coucou",
            //         Name="coucou",
            //         MaturityDate=DateTime.Today,
            //         CurrencyId=cur.Id,
            //         SellCurrencyId=cur.Id,
            //         BuyAmount=10,
            //         IsOtc=true,
            //         PricingFrequency=FrequencyType.Daily
            //     }
            // };

            // var executionOptions = new ExecutionOptions<List<Security>>
            // {
            //     Resolver = new SimpleDependencyResolver().Register<DbContext>(context),
            // };

            // var processRunner = StreamProcessRunner.Create<List<Security>>(i =>
            // {
            //     i.CrossApply("zser", i => i)
            //     .EfCoreSave("qsdf", i => i.SeekOn(j => j.InternalCode));
            // });
            // var res = processRunner.ExecuteAsync(secus, executionOptions).Result;
            // Console.Write(res.Failed ? "Failed" : "Succeeded");








            // var containerBuilder = new ContainerBuilder();
            // containerBuilder.RegisterInstance(new PdfVisitor()).AsImplementedInterfaces();
            // var container = containerBuilder.Build();
            // var etlResolver = new AutofacDependencyResolver(container);
            // etlResolver.TryResolve<IPdfVisitor>(out var tmp);
            // etlResolver.TryResolve<IPdfVisitor>(out tmp);


            var processRunner = StreamProcessRunner.Create<string>(i => i.EfCoreSelectSingle("dfgfsd", (ctx, j) => ctx.Set<Tmp>()));
            var ds = processRunner.GetDefinitionStructure();
            processRunner.ExecuteAsync("ezer").Wait();
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

            // using (var stream = File.OpenRead("InputFiles/TestPdf.pdf"))
            // {
            //     var pdfReader = new PdfReader(stream, null, null, ExtractMethod.SimpleLines(), new Areas
            //     {
            //         ["1"] = new PdfZone { Left = 11.67, Width = 6.43, Top = 29.7 - 2.123, Height = 3.3 },
            //         ["2"] = new PdfZone { Left = 11.67, Width = 6.43, Top = 29.7 - 3.962, Height = 3.3 }
            //     });
            //     var pdfVisitor = new PdfVisitor();
            //     pdfReader.Read(pdfVisitor);
            // }

            // var dpis = new DirectoryInfo("/home/stephane/Downloads/IN").EnumerateFiles("*.pdf").Select(fileInfo => new { FileName = fileInfo.Name, Dpi = GetDpi(fileInfo.OpenRead()) }).ToList();
            // foreach (var dpi in dpis)
            //     Console.WriteLine($"{dpi.FileName}\t{dpi.Dpi}");
        }
        private static int GetDpi(Stream stream)
        {
            using (var pdfDocument = PdfDocument.Open(stream))
                return (int)pdfDocument.GetPages().SelectMany(page => page.GetImages()).Select(img => GetPpi(img)).Average(i => (i.xPpi + i.yPpi) / 2);
        }
        // private readonly HashSet<ColorSpace> _blackAndWhiteColorSpaces=new HashSet<ColorSpace>{ ColorSpace.CalGray, ColorSpace.DeviceGray };
        private static (int xPpi, int yPpi, bool color, int psize) GetPpi(IPdfImage img) => ((int)(72 * img.WidthInSamples / img.Bounds.Width), (int)(72 * img.HeightInSamples / img.Bounds.Height), img.ColorSpace == ColorSpace.DeviceGray, img.BitsPerComponent);
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
public class Tmp
{

}