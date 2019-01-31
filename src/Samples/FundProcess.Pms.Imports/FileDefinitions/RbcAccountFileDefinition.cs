using FundProcess.Pms.Imports.StreamTypes.InputOutput;
using Paillave.Etl.ExcelFile.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.FileDefinitions
{
    public class RbcAccountFileDefinition : ExcelFileDefinition<RbcAccountFile>
    {
        public RbcAccountFileDefinition()
        {
            this.WithMap(i => new RbcAccountFile
            {
                AccountName = i.ToColumn<string>("Account Name"),
                AccountSortName = i.ToColumn<string>("Account Sort Name"),
                Dealer = i.ToColumn<string>("Dealer"),
                RegisterAccount = i.ToColumn<string>("Register Account")
            });
            this.HasColumnHeader("A11:M11");
            this.WithDataset("A12:M12");
        }
    }
}
