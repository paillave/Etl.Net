using System;
using FundProcess.Pms.Imports.StreamTypes;
using FundProcess.Pms.Imports.StreamTypes.InputOutput;
using Paillave.Etl.TextFile.Core;

namespace FundProcess.Pms.Imports.FileDefinitions
{
    public class SetupBdlSicavFileDefinition : FlatFileDefinition<SetupBdlSicavFile>
    {
        public SetupBdlSicavFileDefinition()
        {
            this.IsColumnSeparated(',');
            this.WithMap(i => new SetupBdlSicavFile
            {
                SubFundCode = i.ToColumn<string>("SubFundCode"),
                //SubFundName = i.ToColumn<string>("SubFundName"),
                SicavCode = i.ToColumn<string>("SicavCode"),
                SicavName = i.ToColumn<string>("SicavName"),
            });
        }
    }
}