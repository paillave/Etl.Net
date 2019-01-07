using FundProcess.Pms.DataAccess;

namespace FundProcess.Pms.Imports.StreamTypes
{
    public class ImportFilesConfig : ImportFilesConfigBase
    {
        public DatabaseContext DbCtx { get; set; }
    }
}