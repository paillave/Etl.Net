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
                .CrossApplyFolderFiles("get all Nav files", i => i.InputFilesRootFolderPath, "*NAVPUBLTEXTRACT*.csv", true)
                .CrossApplyTextFile("parse nav file", new RbcNavFileDefinition())
                .ThroughAction("write nav to output", i => Console.WriteLine(i.IsinCode));

            config
                .CrossApplyFolderFiles("get all position files", i => i.InputFilesRootFolderPath, "*PORTFVALEXTRACT*.csv", true)
                .CrossApplyTextFile("parse position file", new RbcPositionFileDefinition(), i => i.Name)
                .ThroughAction("write position to output", i => Console.WriteLine(i.FundName));
        }
    }
}
