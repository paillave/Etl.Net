using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.StreamTypes.InputOutput
{
    public class BdlSecPosNode // /response/response/cont/pos/Secpos
    {
        public string ContId { get; set; } // /response/response/cont/@ContId
        public string SecurityCode { get; set; } // <SecurityCode>24493476</SecurityCode>
        //public string Isin { get; set; } // <Isin>BE0003851681</Isin>
        //public string DepositaryBic { get; set; } // <DepositaryBic>PARBFRPP</DepositaryBic>
        //public string DepositaryName { get; set; } // <DepositaryName>BNP PARIBAS SECURITIES SERVICES PARIS</DepositaryName>
        public decimal AssetQty { get; set; } // <AssetQty>2190</AssetQty>
        public DateTime AssBalDate { get; set; } // <AssBalDate>2018-07-04</AssBalDate>
        //public string PriceCcy { get; set; } // <PriceCcy>EUR</PriceCcy>
        //public string UnitPrice { get; set; } // <UnitPrice>78.4000000</UnitPrice>
        //public string PriceDate { get; set; } // <PriceDate>2018-07-04</PriceDate>
        //public string CostPrice { get; set; } // <CostPrice>73.2077854</CostPrice>
        //public string aveCostCcy { get; set; } // <aveCostCcy>EUR</aveCostCcy>
        //public string UnReEur { get; set; } // <UnReEur>11370.95</UnReEur>
        public decimal TotSec { get; set; } // <TotSec>171696.00</TotSec>
        //public string TotSecCcy { get; set; } // <TotSecCcy>EUR</TotSecCcy>
        public decimal TotRefCcy { get; set; } // <TotRefCcy>171696.00</TotRefCcy>
        //public string TotInClientCcy { get; set; } // <TotInClientCcy>171696.00</TotInClientCcy>
        //public string ClientCcy { get; set; } // <ClientCcy>EUR</ClientCcy>
        //public string AccrIntRef { get; set; } // <AccrIntRef>0</AccrIntRef>
        //public string RefCcy { get; set; } // <RefCcy>EUR</RefCcy>
        //public string PosBlock { get; set; } // <PosBlock/>
        //public string QtyBlock { get; set; } // <QtyBlock>0</QtyBlock>
        //public string CollectionId { get; set; } // <CollectionId>53338050.3</CollectionId>
    }
}
