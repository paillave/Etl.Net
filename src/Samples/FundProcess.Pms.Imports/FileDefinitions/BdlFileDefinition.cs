using FundProcess.Pms.Imports.StreamTypes.InputOutput;
using Paillave.Etl.XmlFile.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.FileDefinitions
{
    public class BdlFileDefinition : XmlFileDefinition
    {
        public BdlFileDefinition()
        {
            this.AddNodeDefinition(XmlNodeDefinition.Create("secbase", "/response/response/cont/pos/Secbase", i => new BdlSecBaseNode
            {
                Bloomberg = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/Bloomberg"),
                Domicile = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/Domicile"),
                InstrCcy = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/InstrCcy"),
                InstrType = i.ToXPathQuery<int>("/response/response/cont/pos/Secbase/InstrType"),
                Isin = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/Isin"),
                MifidRisk = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/MifidRisk"),
                Price = i.ToXPathQuery<decimal>("/response/response/cont/pos/Secbase/Price"),
                PriceCcy = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/PriceCcy"),
                PriceDate = i.ToXPathQuery<DateTime>("/response/response/cont/pos/Secbase/PriceDate"),
                Reuters = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/Reuters"),
                SecName = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/SecName"),
                SecurityCode = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/SecurityCode"),
                Telekurs = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/Telekurs"),
                ValFreq = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/ValFreq"),
                Wkn = i.ToXPathQuery<string>("/response/response/cont/pos/Secbase/Wkn")
            }));
            this.AddNodeDefinition(XmlNodeDefinition.Create("secpos", "/response/response/cont/pos/Secpos", i => new BdlSecPosNode
            {
                ContId = i.ToXPathQuery<string>("/response/response/cont/@ContId"),
                AssBalDate = i.ToXPathQuery<DateTime>("/response/response/cont/pos/Secpos/AssBalDate"),
                AssetQty = i.ToXPathQuery<decimal>("/response/response/cont/pos/Secpos/AssetQty"),
                SecurityCode = i.ToXPathQuery<string>("/response/response/cont/pos/Secpos/SecurityCode"),
                TotRefCcy = i.ToXPathQuery<decimal>("/response/response/cont/pos/Secpos/TotRefCcy"),
                TotSec = i.ToXPathQuery<decimal>("/response/response/cont/pos/Secpos/TotSec"),
            }));
            this.AddNodeDefinition(XmlNodeDefinition.Create("cashpos", "/response/response/cont/pos/Cashpos", i => new BdlCashposNode
            {
                ContId = i.ToXPathQuery<string>("/response/response/cont/@ContId"),
                AccrInt = i.ToXPathQuery<decimal>("/response/response/cont/pos/Cashpos/AccrInt"),
                AssetCcy = i.ToXPathQuery<string>("/response/response/cont/pos/Cashpos/AssetCcy"),
                Iban = i.ToXPathQuery<string>("/response/response/cont/pos/Cashpos/Iban"),
                PosBalDate = i.ToXPathQuery<DateTime>("/response/response/cont/pos/Cashpos/PosBalDate"),
                PosBalRefCcy = i.ToXPathQuery<decimal>("/response/response/cont/pos/Cashpos/PosBalRefCcy"),
            }));
            //this.AddNodeDefinition(XmlNodeDefinition.Create("cashtr", "/response/response/cont/pos/Cashtr", i => new BdlCashTransactionNode
            //{
            //    ContId = i.ToXPathQuery<string>("/response/response/cont/@ContId"),
            //    BookText = i.ToXPathQuery<string>("/response/response/cont/pos/Cashtr/BookText"),
            //    Iban = i.ToXPathQuery<string>("/response/response/cont/pos/Cashtr/Iban"),
            //    NetAmount = i.ToXPathQuery<decimal>("/response/response/cont/pos/Cashtr/NetAmount"),
            //    OrderNr = i.ToXPathQuery<int>("/response/response/cont/pos/Cashtr/OrderNr"),
            //    OrdTypId = i.ToXPathQuery<int>("/response/response/cont/pos/Cashtr/OrdTypId")
            //}));
        }
    }
}
