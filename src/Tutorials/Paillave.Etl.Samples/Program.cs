using System;
using System.Collections.Generic;
using System.IO;
using Paillave.Pdf;

namespace Paillave.Etl.Samples
{
    class PdfVisitor : IPdfVisitor
    {
        public void ProcessHeader(List<string> section, int pageNumber)
        {
        }

        public void ProcessLine(string text, int pageNumber, int lineNumber, int lineNumberInParagraph, int lineNumberInPage, List<string> section)
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
            using (var stream = File.OpenRead("TestPbLines.pdf"))
            using (var pdfReader = new PdfReader(stream, null, null, ExtractMethod.SimpleLines()))
            {
                pdfReader.Read(new PdfVisitor());
            }
        }
    }
}
