using FundProcess.Pms.Imports.StreamTypes.InputOutput;
using Paillave.Etl.ExcelFile.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.FileDefinitions
{
    public class RbcAccountPositionFileDefinition : ExcelFileDefinition<RbcAccountPositionFile>
    {
        public RbcAccountPositionFileDefinition()
        {
            this.WithMap(i => new RbcAccountPositionFile
            {
                AccountNumber = i.ToColumn<string>("ACCOUNT NUMBER"),
                Isin = i.ToColumn<string>("ISIN code"),
                //Assets = i.ToNumberColumn<decimal>("ASSETS", "."),
                //SharesBalance = i.ToNumberColumn<decimal>("SHARES BALANCE", "."),
                Assets = i.ToColumn<decimal>("ASSETS"),
                SharesBalance = i.ToColumn<decimal>("SHARES BALANCE"),
                HoldingDate = i.ToDateColumn<DateTime>("HOLDING DATE", "MM/dd/yyyy")
            });
            this.HasColumnHeader("A8:I8");
            this.WithDataset("A9:I9");
        }
    }
}
