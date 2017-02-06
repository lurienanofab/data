using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LNF.Data;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Web.Mvc;
using LNF.Web.Mvc.UI;

namespace Data.Models
{
    public class zAccountModel : BaseModel
    {
        public string SearchText { get; set; }
        public int? AccountID { get; set; }

        public override SubMenu GetSubMenu()
        {
            return base.GetSubMenu()
                .Add(new SubMenu.MenuItem() { LinkText = "Add/Modify Accounts", ActionName = "Index", ControllerName = "Account" })
                .Add(new SubMenu.MenuItem() { LinkText = "Edit", ActionName = "Edit", ControllerName = "Account" });
        }

        public IEnumerable<Account> GetAllAccounts()
        {
            return DA.Current.Query<Account>().OrderBy(x => x.Name);
        }
    }
}