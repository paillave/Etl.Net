using FundProcess.Pms.DataAccess;
using Paillave.Etl.Config;
using Paillave.Etl.EntityFrameworkCore.Config;

namespace FundProcess.Pms.Imports.StreamTypes.Config
{
    public class RbcImportAccountFilesConfigCtx : IConfigWithRootPath, IConfigDbContext<DatabaseContext>
    {
        public DatabaseContext DbContext { get; set; }
        public string AccountFileNamePattern { get; set; }
        public string AccountPositionFileNamePattern { get; set; }
        public string InputFilesRootFolderPath { get; set; }
    }
}