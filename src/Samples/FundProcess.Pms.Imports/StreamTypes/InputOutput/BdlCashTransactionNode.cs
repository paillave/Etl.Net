using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.StreamTypes.InputOutput
{
    public class BdlCashTransactionNode // /response/response/cont/pos/Cashtr
    {
        public string ContId { get; set; } // /response/response/cont/@ContId
        public string Iban { get; set; }// <Iban>LU230080291389002001</Iban>
        public int OrderNr { get; set; }// <OrderNr>236248234</OrderNr>
        //public string MaturityOrderNr { get; set; }// <MaturityOrderNr/>
        public int OrdTypId { get; set; }// <OrdTypId>140024</OrdTypId>
        //public string ExternalRef { get; set; }// <ExternalRef/>
        public string BookText { get; set; }// <BookText>Redemption (Funds) Equity funds</BookText>
        //public string ContractNr { get; set; }// <ContractNr/>
        //public string GrossAmt { get; set; }// <GrossAmt/>
        //public string GrossAmtCcy { get; set; }// <GrossAmtCcy/>
        //public string Xrate { get; set; }// <Xrate/>
        //public string fwdRate { get; set; }// <fwdRate/>
        //public string NdfFixTime { get; set; }// <NdfFixTime/>
        //public string NdfFixRate { get; set; }// <NdfFixRate/>
        //public string NdfShortAmt { get; set; }// <NdfShortAmt/>
        //public string NdfShortAmtCcy { get; set; }// <NdfShortAmtCcy/>
        //public string NdfMaturityDate { get; set; }// <NdfMaturityDate/>
        //public string NdfLongAmt { get; set; }// <NdfLongAmt/>
        //public string NdfLongAmtCcy { get; set; }// <NdfLongAmtCcy/>
        //public string NdfValuePrice { get; set; }// <NdfValuePrice/>
        //public string NdfConclDate { get; set; }// <NdfConclDate/>
        //public string BookCcy { get; set; }// <BookCcy>EUR</BookCcy>
        //public string FeeComm { get; set; }// <FeeComm/>
        public decimal NetAmount { get; set; }// <NetAmount>784250.40</NetAmount>
        //public string bookDate { get; set; }// <bookDate>2018-07-03</bookDate>
        //public string ValDate { get; set; }// <ValDate>2018-07-06</ValDate>
        //public string ReverseInd { get; set; }// <ReverseInd/>
        //public string ReversalCode { get; set; }// <ReversalCode/>
        //public string CountryO { get; set; }// <CountryO/>
        //public string Communication1 { get; set; }// <Communication1/>
        //public string Communication2 { get; set; }// <Communication2/>
        //public string Communication3 { get; set; }// <Communication3/>
        //public string Communication4 { get; set; }// <Communication4/>
        //public string PayBenIban { get; set; }// <PayBenIban/>
        //public string PayBenAddr1 { get; set; }// <PayBenAddr1/>
        //public string PayBenAddr2 { get; set; }// <PayBenAddr2/>
        //public string PayBenAddr3 { get; set; }// <PayBenAddr3/>
        //public string PayBenAddr4 { get; set; }// <PayBenAddr4/>
        //public string PerfDate { get; set; }// <PerfDate>2018-07-04</PerfDate>
        //public string VerifDate { get; set; }// <VerifDate>2018-07-04</VerifDate>
    }
}
