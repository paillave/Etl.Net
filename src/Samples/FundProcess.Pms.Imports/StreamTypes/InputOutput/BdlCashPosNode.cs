using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.StreamTypes.InputOutput
{
    public class BdlCashposNode // /response/response/cont/pos/Cashpos
    {
        public string ContId { get; set; } // /response/response/cont/@ContId
        public string Iban { get; set; } // <Iban>LU200080281260102005</Iban>
        //public string AssetType { get; set; } // <AssetType>9100</AssetType>
        public string AssetCcy { get; set; } // <AssetCcy>USD</AssetCcy>
        //public string balVal { get; set; } // <balVal>23.75</balVal>
        //public string balBook { get; set; } // <balBook>23.75</balBook>
        public DateTime PosBalDate { get; set; } // <PosBalDate>2018-07-04</PosBalDate>
        public decimal PosBalRefCcy { get; set; } // <PosBalRefCcy>20.37</PosBalRefCcy>
        //public string CreditRate { get; set; } // <CreditRate>0</CreditRate>
        //public string DebitRate { get; set; } // <DebitRate>10.75000</DebitRate>
        //public string StartDate { get; set; } // <StartDate>2015-04-29</StartDate>
        //public string MatDate { get; set; } // <MatDate/>
        public decimal AccrInt { get; set; } // <AccrInt>0.00</AccrInt>
        //public string IntMat { get; set; } // <IntMat/>
    }
}
