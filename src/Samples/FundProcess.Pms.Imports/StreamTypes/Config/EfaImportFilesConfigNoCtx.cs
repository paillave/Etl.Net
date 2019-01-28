
using FundProcess.Pms.DataAccess;
using Paillave.Etl.Config;

namespace FundProcess.Pms.Imports.StreamTypes.Config
{
    public class EfaImportFilesConfigNoCtx : IConfigWithRootPath, IConfigMultiTenantDatabaseServer
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public int EntityId { get; set; }
        public int EntityGroupId { get; set; }
        public string NavFileFileNamePattern { get; set; }
        public string PositionFileFileNamePattern { get; set; }
        public string InputFilesRootFolderPath { get; set; }
    }
}