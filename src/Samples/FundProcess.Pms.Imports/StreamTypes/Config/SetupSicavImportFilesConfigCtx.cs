using FundProcess.Pms.DataAccess;
using Paillave.Etl.Config;
using Paillave.Etl.EntityFrameworkCore.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.StreamTypes.Config
{
    public class SetupSicavImportFilesConfigCtx : IConfigWithRootPath, IConfigDbContext<DatabaseContext>
    {
        public string InputFilesRootFolderPath { get; set; }
        public string FileNamePattern { get; set; }
        public DatabaseContext DbContext { get; set; }
    }
}
