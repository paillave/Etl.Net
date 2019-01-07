
using FundProcess.Pms.DataAccess;

namespace FundProcess.Pms.Imports.StreamTypes
{
    public class ImportFilesConfigNoCtx : ImportFilesConfigBase
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public int EntityId { get; set; }
        public int EntityGroupId { get; set; }
    }
}