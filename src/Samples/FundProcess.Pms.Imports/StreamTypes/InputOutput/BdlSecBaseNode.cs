using System;
using System.Collections.Generic;
using System.Text;

namespace FundProcess.Pms.Imports.StreamTypes.InputOutput
{
    public class BdlSecBaseNode // <Secbase>
    {
        public string SecurityCode { get; set; } // <SecurityCode>13555251</SecurityCode>
        public string Isin { get; set; } // <Isin>FR0010400762</Isin>
        public string Telekurs { get; set; } // <Telekurs>2838572</Telekurs>
        public string Reuters { get; set; } // <Reuters/>
        public string Bloomberg { get; set; } // <Bloomberg/>
        public string Wkn { get; set; } // <Wkn>A1CV0R</Wkn>
        public string SecName { get; set; } // <SecName>Moneta Asset Management Moneta Long Short - A CAP</SecName>
        //public string CFIclass { get; set; } // <CFIclass>EUOGMB</CFIclass>
        public int InstrType { get; set; } // <InstrType>880</InstrType>
        //public int InstrTypeL1 { get; set; } // <InstrTypeL1>15224</InstrTypeL1>
        //public int InstrTypeL2 { get; set; } // <InstrTypeL2>15235</InstrTypeL2>
        //public int InstrTypeL3 { get; set; } // <InstrTypeL3/>
        //public string TradingPlace { get; set; } // <TradingPlace>68899</TradingPlace>
        //public string EconSector { get; set; } // <EconSector>19</EconSector>
        //public string GeoSector { get; set; } // <GeoSector>01</GeoSector>
        public string Domicile { get; set; } // <Domicile>FR</Domicile>
        public string InstrCcy { get; set; } // <InstrCcy>EUR</InstrCcy>
        //public string PriceUnit { get; set; } // <PriceUnit>mon</PriceUnit>
        //public int QuotMode { get; set; } // <QuotMode>11</QuotMode>
        //public string PriceCcy { get; set; } // <PriceCcy>EUR</PriceCcy>
        //public decimal Price { get; set; } // <Price>166.63</Price>
        //public DateTime PriceDate { get; set; } // <PriceDate>2016-12-16</PriceDate>
        //public DateTime IssueDate { get; set; } // <IssueDate>2006-12-15</IssueDate>
        //public DateTime MatRdmptDate { get; set; } // <MatRdmptDate/>
        //public decimal RdmptPrice { get; set; } // <RdmptPrice/>
        //public string Issuer { get; set; } // <Issuer>Moneta Asset Management Moneta Long Short FCP</Issuer>
        //public string IssueDomic { get; set; } // <IssueDomic>FR</IssueDomic>
        //public decimal IntrRate { get; set; } // <IntrRate/>
        //public string IntrCalcBasis { get; set; } // <IntrCalcBasis/>
        //public string AccrIntDays { get; set; } // <AccrIntDays/>
        //public string IntDaysPA { get; set; } // <IntDaysPA/>
        //public string YieldToMat { get; set; } // <YieldToMat/>
        //public decimal FaceAmt { get; set; } // <FaceAmt/>
        //public DateTime CoupPeriod { get; set; } // <CoupPeriod/>
        //public DateTime LastCoupDate { get; set; } // <LastCoupDate/>
        //public DateTime NextCoupDate { get; set; } // <NextCoupDate/>
        //public string FutTick { get; set; } // <FutTick/>
        //public int ContrSize { get; set; } // <ContrSize/>
        //public DateTime ExpDate { get; set; } // <ExpDate/>
        //public decimal StrikePrice { get; set; } // <StrikePrice/>
        //public string EurAmStyle { get; set; } // <EurAmStyle/>
        //public string UnderlAsset { get; set; } // <UnderlAsset/>
        //public string AssetBlock { get; set; } // <AssetBlock/>
        //public string EusdTis { get; set; } // <EusdTis/>
        //public string EusdStatA { get; set; } // <EusdStatA>16530</EusdStatA>
        //public string CaaClassif { get; set; } // <CaaClassif>CAA-D01</CaaClassif>
        //public string FundTradeDays { get; set; } // <FundTradeDays/>
        //public string SubFreq { get; set; } // <SubFreq>7310</SubFreq>
        //public string RedFreq { get; set; } // <RedFreq>7310</RedFreq>
        //public string DistrAgr { get; set; } // <DistrAgr>18832</DistrAgr>
        //public string ClassicFd { get; set; } // <ClassicFd>18793</ClassicFd>
        //public string ApplicForm { get; set; } // <ApplicForm>7333</ApplicForm>
        //public decimal FundValRdmpt { get; set; } // <FundValRdmpt/>
        //public decimal FundValSubs { get; set; } // <FundValSubs/>
        //public decimal SubInQtAmt { get; set; } // <SubInQtAmt>7300</SubInQtAmt>
        //public decimal RedInQtAmt { get; set; } // <RedInQtAmt>7300</RedInQtAmt>
        //public decimal MinSubsAmt { get; set; } // <MinSubsAmt/>
        //public decimal MinSubsqAmt { get; set; } // <MinSubsqAmt/>
        //public decimal QtyNrDec { get; set; } // <QtyNrDec>4</QtyNrDec>
        //public string TechCutoff { get; set; } // <TechCutoff/>
        //public string FrontEndLoad { get; set; } // <FrontEndLoad/>
        //public string BackEndLoad { get; set; } // <BackEndLoad/>
        //public string NegFrontEndLoad { get; set; } // <NegFrontEndLoad/>
        //public string NegBackEndLoad { get; set; } // <NegBackEndLoad/>
        public string ValFreq { get; set; } // <ValFreq>7310</ValFreq> 7310=daily, 7312=weekly
        public string MifidRisk { get; set; } // <MifidRisk>Risk Level 4 (04)</MifidRisk>
        //public string MifidComplx { get; set; } // <MifidComplx>Complex (01)</MifidComplx>
        //public string WthldTax { get; set; } // <WthldTax>10058</WthldTax>
        //public string PoolFactor { get; set; } // <PoolFactor/>
    }
}
