using FundProcess.Pms.DataAccess;
using Paillave.Etl.Config;
using Paillave.Etl.EntityFrameworkCore.Config;

namespace FundProcess.Pms.Imports.StreamTypes.Config
{
    public class EfaImportFilesConfigCtx : IConfigWithRootPath, IConfigDbContext<DatabaseContext>
    {
        public DatabaseContext DbContext { get; set; }
        public string NavFileFileNamePattern { get; set; }
        public string PositionFileFileNamePattern { get; set; }
        public string InputFilesRootFolderPath { get; set; }
    }
}