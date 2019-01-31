using Paillave.Etl.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.StreamTypes.Config
{
    public class BdlImportFilesConfigNoCtx : IConfigMultiTenantDatabaseServer, IConfigWithRootPath
    {
        public string InputFilesRootFolderPath { get; set; }
        public int EntityId { get; set; }
        public int EntityGroupId { get; set; }
        public string Database { get; set; }
        public string Server { get; set; }
        public string FileNamePattern { get; set; }
    }
}
