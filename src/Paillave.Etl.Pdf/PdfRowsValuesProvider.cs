using System;
using Paillave.Etl.Core;
using System.Collections.Generic;
using System.Threading;
// using Paillave.Etl.ValuesProviders;
using Paillave.Pdf;

namespace Paillave.Etl.Pdf
{
    public class PdfRowsValuesProviderArgs
    {
        public IList<TextTemplate> PatternsToIgnore { get; } = new List<TextTemplate>();
        public IList<HeadersSetup> HeadersSetups { get; } = new List<HeadersSetup>();
        public ExtractMethod ExtractMethod { get; set; } = null;
        public Areas Areas { get; set; } = new Areas();
        public PdfRowsValuesProviderArgs AddHeadersSetup(HeadersSetup headersSetup)
        {
            this.HeadersSetups.Add(headersSetup);
            return this;
        }
        public PdfRowsValuesProviderArgs SetArea(string code, double left, double width, double top, double height, int? pageNumber = null)
        {
            this.Areas[code] = new PdfZone
            {
                Left = left,
                Width = width,
                Top = top,
                Height = height,
                PageNumber = pageNumber
            };
            return this;
        }
        public PdfRowsValuesProviderArgs AddIgnore(Func<TextTemplate, TextTemplate> templateBuilder)
        {
            this.PatternsToIgnore.Add(templateBuilder(new TextTemplate()));
            return this;
        }
        public PdfRowsValuesProviderArgs SetExtractMethod(ExtractMethod extractMethod)
        {
            this.ExtractMethod = extractMethod;
            return this;
        }
    }
    public abstract class PdfContent
    {
        protected PdfContent(List<string> section, int pageNumber, IFileValue fileValue)
            => (Section, PageNumber, FileValue) = (section, pageNumber, fileValue);
        public List<string> Section { get; }
        public int PageNumber { get; }
        public IFileValue FileValue { get; }
    }
    public class PdfHeader : PdfContent
    {
        public PdfHeader(IFileValue fileValue, List<string> section, int pageNumber) : base(section, pageNumber, fileValue) { }
    }
    public class PdfTable : PdfContent
    {
        public List<List<List<string>>> Table { get; }
        public PdfTable(IFileValue fileValue, List<string> section, int pageNumber, List<List<List<string>>> table) : base(section, pageNumber, fileValue) => (Table) = (table);
    }
    public class PdfTextLine : PdfContent
    {
        public string Text { get; }
        public int LineNumber { get; }
        public string AreaCode { get; }
        public PdfTextLine(IFileValue fileValue, List<string> section, int pageNumber, int lineNumber, string text, string areaCode) : base(section, pageNumber, fileValue) => (Text, LineNumber, AreaCode) = (text, lineNumber, areaCode);
    }
    public class PdfRowsValuesProvider : ValuesProviderBase<IFileValue, PdfContent>
    {
        private readonly PdfRowsValuesProviderArgs _args;
        public PdfRowsValuesProvider(PdfRowsValuesProviderArgs args) => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;
        public override void PushValues(IFileValue input, Action<PdfContent> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var stream = input.GetContent();
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            var pdfReader = new PdfReader(stream, this._args.PatternsToIgnore, this._args.HeadersSetups, this._args.ExtractMethod);
            pdfReader.Read(new PdfVisitor(push, input));
        }
    }
    public class PdfVisitor : IPdfVisitor
    {
        private readonly Action<PdfContent> _push;
        private readonly IFileValue _fileValue;
        public PdfVisitor(Action<PdfContent> push, IFileValue fileValue) => (_push, _fileValue) = (push, fileValue);
        public void ProcessLine(string text, int pageNumber, int lineNumber, int lineNumberInParagraph, int lineNumberInPage, List<string> section, string area)
            => _push(new PdfTextLine(_fileValue, section, pageNumber, lineNumber, text, area));
        public void ProcessTable(List<List<List<string>>> table, int pageNumber, List<string> section)
            => _push(new PdfTable(_fileValue, section, pageNumber, table));
        public void ProcessHeader(List<string> section, int pageNumber)
            => _push(new PdfHeader(_fileValue, section, pageNumber));
    }
}
