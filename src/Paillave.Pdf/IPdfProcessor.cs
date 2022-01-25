using System.Collections.Generic;

namespace Paillave.Pdf
{
    public interface IPdfProcessor
    {
        void ProcessLine(string text, int pageNumber, int lineNumber, int lineNumberInParagraph, int lineNumberInPage, List<string> section);
        void ProcessHeader(List<string> section, int pageNumber);
        void ProcessTable(List<List<List<string>>> table, int pageNumber, List<string> section);
    }
}
