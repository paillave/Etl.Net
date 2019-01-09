using FundProcess.Pms.DataAccess;

namespace FundProcess.Pms.Imports.StreamTypes
{
    public abstract class ImportFilesConfigBase
    {
        public string InputFilesRootFolderPath { get; set; }
        public string NavFileFileNamePattern { get; set; } = "*NAVPUBLTEXTRACT*.csv";
        public string PositionFileFileNamePattern { get; set; } = "*PORTFVALEXTRACT*.csv";
    }
}