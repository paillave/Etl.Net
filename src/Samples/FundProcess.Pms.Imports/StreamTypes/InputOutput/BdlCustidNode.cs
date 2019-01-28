using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.StreamTypes.InputOutput
{
    public class BdlCustidNode
    {
        public string ContId { get; set; } // <cont ContId="02812601.1001">
        //public string InsurContNr { get; set; } // <InsurContNr/>
        //public string CollectionId { get; set; } // <CollectionId>53440040.3</CollectionId>
        public string Domicile { get; set; } // <Domicile>MC</Domicile>
        public string DefaultCcy { get; set; } // <DefaultCcy>EUR</DefaultCcy>
        //public DateTime OpenDate { get; set; } // <OpenDate>2013-03-20</OpenDate>
        //public string TaxDomicile { get; set; } // <TaxDomicile>MC</TaxDomicile>
        //public int EusdStatus { get; set; } // <EusdStatus>2</EusdStatus>
        //public string InitMifid { get; set; } // <InitMifid>Privé (01)</InitMifid>
        //public int FinKnowl { get; set; } // <FinKnowl>5</FinKnowl>
        //public int FinExp { get; set; } // <FinExp>5</FinExp>
    }
}
