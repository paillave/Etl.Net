using System;
using FundProcess.Pms.Imports.StreamTypes;
using FundProcess.Pms.Imports.StreamTypes.InputOutput;
using Paillave.Etl.TextFile.Core;

namespace FundProcess.Pms.Imports.FileDefinitions
{
    public class EfaPositionFileDefinition : FlatFileDefinition<EfaPositionFile>
    {
        public EfaPositionFileDefinition()
        {
            this.IsColumnSeparated(',');
            this.WithMap(i => new EfaPositionFile
            {
                FundCode = i.ToColumn<string>("Fund_code"),
                FundLongName = i.ToColumn<string>("Fund_Long_Name"),
                SubFundCode = i.ToColumn<string>("Sub-Fund_Code"),
                SubFundName = i.ToColumn<string>("Sub-Fund_long_name"),
                SubFundCurrency = i.ToColumn<string>("Sub-Fund_ccy"),
                ValuationDate = i.ToDateColumn<DateTime>("Valuation_date", "dd/MM/yyyy"),
                InstrumentCode = i.ToColumn<string>("Instr_code"),
                InstrumentCategory = i.ToColumn<string>("Instr_Category"),
                InstrumentCurrency = i.ToColumn<string>("Instr_evaluation_ccy"),
                InstrumentName = i.ToColumn<string>("Instr_long_name"),
                InstrumentIsin = i.ToColumn<string>("ISIN"),
                BloombergCode = i.ToColumn<string>("Bloomberg Code"),
                Category1 = i.ToColumn<string>("Category_1"),
                Category2 = i.ToColumn<string>("Category_2"),
                MarketValue = i.ToNumberColumn<decimal>("Market_Value", "."),
                MarketValueInInstrumentCurrency = i.ToNumberColumn<decimal>("Market_Value_in_Instr_CCY", "."),
                Quantity = i.ToNumberColumn<decimal>("Quantity", ".")
            });
        }
    }
}
