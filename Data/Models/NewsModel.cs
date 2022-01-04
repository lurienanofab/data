using LNF.Web.Mvc;
using LNF.Web.Mvc.UI;
using System.Web;

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