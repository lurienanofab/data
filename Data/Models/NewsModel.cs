using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LNF.Web.Mvc;
using LNF.Web.Mvc.UI;

namespace Data.Models
{
    public class NewsModel : BaseModel
    {
        public int NewsID { get; set; }

        public override SubMenu GetSubMenu()
        {
            return base.GetSubMenu()
                .Add(new SubMenu.MenuItem() { LinkText = "Edit", ActionName = "Edit", ControllerName = "News" })
                .Add(new SubMenu.MenuItem() { LinkText = "View", ActionName = "View", ControllerName = "News" });
        }

        public IHtmlString Title()
        {
            string result = "Lurie Nanofabrication Facility";
            return new HtmlString(result);
        }
    }
}