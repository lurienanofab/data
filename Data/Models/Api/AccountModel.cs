using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Data.Models.Api
{
    public class AccountModel
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public string ShortCode { get; set; }
    }

    public class AccountModelComparer: IEqualityComparer<AccountModel>
    {
        public bool Equals(AccountModel x, AccountModel y)
        {
            return x.AccountID == y.AccountID;
        }

        public int GetHashCode(AccountModel obj)
        {
            return obj.AccountID.GetHashCode();
        }
    }
}