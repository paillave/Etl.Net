using System;
using TestCsv.StreamTypes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Extensions;
using Paillave.Etl.TextFile.Extensions;
using TestCsv.FileDefinitions;

namespace TestCsv.Jobs
{
    public class ImportFiles
    {
        public static void DefineProcess(ISingleStream<ImportFilesConfig> config)
        {
            config
                .CrossApplyFolderFiles("get all NAVPUBLTEXTRACT", i => i.InputFilesRootFolderPath, "*NAVPUBLTEXTRACT*.csv", true)
                .CrossApplyTextFile("parse NAVPUBLTEXTRACT", new RbcNavPublExtractDefinition(), i => i.Name)
                .ThroughAction("write to output", i => Console.WriteLine(i.IsinCode));
        }
    }
}
