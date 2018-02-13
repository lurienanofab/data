using System;
using System.Collections.Generic;

namespace Data.Models
{
    public class AccountEdit
    {
        public int OrgID { get; set; }
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string ShortCode { get; set; }
        public int FundingSourceID { get; set; }
        public int TechnicalFieldID { get; set; }
        public int SpecialTopicID { get; set; }
        public int AccountTypeID { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceLine1 { get; set; }
        public string InvoiceLine2 { get; set; }
        public DateTime? PoEndDate { get; set; }
        public decimal? PoInitialFunds { get; set; }
        public decimal? PoRemainingFunds { get; set; }
        public IDictionary<string, AddressEdit> Addresses { get; set; }
        public IEnumerable<AccountManagerEdit> Managers { get; set; }
        public AccountChartFieldEdit ChartFields { get; set; }
    }
}