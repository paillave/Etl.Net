using System;

namespace FundProcess.Pms.Imports.StreamTypes.InputOutput
{
    public class RbcSecurityTransactionFile
    {
        public string ConsolidatedCode { get; set; }
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public string FundCcy { get; set; }
        public int CnPtfManager { get; set; }
        public string OperationType { get; set; }
        public string DealDescription { get; set; }
        public int NbEntryPtf { get; set; }
        public int NbEntry { get; set; }
        public string SecurityMvtStatusCode { get; set; }
        public string CNId { get; set; }
        public string WknCode { get; set; }
        public string IsinCode { get; set; }
        public string SecName { get; set; }
        public string GTICode { get; set; }
        public string InvestmentCcy { get; set; }
        public DateTime MaturityDate { get; set; }
        public string ContractNumber { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime NavDate { get; set; }
        public decimal Quantity { get; set; }
        public string PriceLocalCcy { get; set; }
        public string PricePtfCcy { get; set; }
        public string FeesLocalCcy { get; set; }
        public decimal PurchAmountLocalCcyGross { get; set; }
        public decimal InterestsPurchasedLocalCcy { get; set; }
        public decimal PurchAmountLocalCcyNet { get; set; }
        public decimal PurchAmountFundCurrencyNet { get; set; }
        public decimal SalesAmountLocalCurrencyGross { get; set; }
        public decimal InterestSoldLocalCcy { get; set; }
        public string SalesAmountLocalCcyNet { get; set; }
        public string SalesAmountFundCcyNet { get; set; }
        public decimal TotalGainOrLoss { get; set; }
        public int SecurityAccountingEntryNumber { get; set; }
        public decimal XchgeRate { get; set; }
    }
}