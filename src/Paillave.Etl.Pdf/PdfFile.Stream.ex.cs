using Paillave.Etl.Core;
using System;

namespace Paillave.Etl.Pdf
{
    public static class PdfFileEx
    {
        public static IStream<PdfContent> CrossApplyPdfContent(this IStream<IFileValue> stream, string name, Func<PdfRowsValuesProviderArgs, PdfRowsValuesProviderArgs> argsBuilder, bool noParallelisation = false)
            => stream.CrossApply(name, new PdfRowsValuesProvider(argsBuilder(new PdfRowsValuesProviderArgs())), noParallelisation);
    }
}
