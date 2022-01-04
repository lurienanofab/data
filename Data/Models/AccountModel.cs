using LNF.Data;
using System.Collections.Generic;

namespace Data.Models
{
    public class AccountModel
    {
        public IOrg CurrentOrg { get; set; }
        public IEnumerable<IOrg> ActiveOrgs { get; set; }
        public IEnumerable<IAccount> ActiveAccounts { get; set; }
        public bool IsChartFieldOrg { get; set; }
    }
}