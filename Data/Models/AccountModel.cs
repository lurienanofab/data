using LNF.Repository.Data;
using System.Collections.Generic;

namespace Data.Models
{
    public class AccountModel
    {
        public Org CurrentOrg { get; set; }
        public IEnumerable<Org> ActiveOrgs { get; set; }
        public IEnumerable<Account> ActiveAccounts { get; set; }
        public bool IsChartFieldOrg { get; set; }
    }
}