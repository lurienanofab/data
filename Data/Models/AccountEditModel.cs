using LNF.Repository.Data;
using System.Collections.Generic;

namespace Data.Models
{
    public class AccountEditModel
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public Org CurrentOrg { get; set; }
        public IEnumerable<FundingSource> FundingSources { get; set; }
        public IEnumerable<TechnicalField> TechnicalFields { get; set; }
        public IEnumerable<SpecialTopic> SpecialTopics { get; set; }
        public IEnumerable<AccountManagerEdit> AvailableManagers { get; set; }
        public bool IsChartFieldOrg { get; set; }
    }
}