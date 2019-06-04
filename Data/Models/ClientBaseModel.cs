using LNF.Models.Data;
using LNF.Web.Mvc;
using LNF.Web.Mvc.UI;
using System;

namespace Data.Models
{
    public abstract class ClientBaseModel : BaseModel
    {
        public override SubMenu GetSubMenu()
        {
            //TODO: use the cookie mechanism

            //string qs = (App == "sselData") ? "?app=sselData" : string.Empty;
            //string homeUrl = (App == "sselData") ? "/sselData" : VirtualPathUtility.ToAbsolute("~");
            //string clientUrl = (App == "sselData") ? "/sselData/Client.aspx" : VirtualPathUtility.ToAbsolute("~/client");

            bool temp = false;
            if (temp)
                throw new NotImplementedException();

            return base.GetSubMenu()
                .Add(new SubMenu.MenuItem() { LinkText = "Add/Modify Client", ActionName = "", ControllerName = "" })
                .Add(new SubMenu.MenuItem() { LinkText = "Account Assignment", ActionName = "", ControllerName = "" })
                .Add(new SubMenu.MenuItem() { LinkText = "Access", ActionName = "", ControllerName = "" })
                .Add(new SubMenu.MenuItem() { LinkText = "Password Reset", ActionName = "", ControllerName = "" });
        }
    }
}