using System;
using System.Configuration;
using System.Web.Mvc;

namespace Data
{
    public static class HtmlHelperExtensions
    {
        public static bool ShowMenu(this HtmlHelper helper)
        {
            bool showMenu = string.IsNullOrEmpty(Convert.ToString(helper.ViewContext.HttpContext.Session["return-to"])) && !bool.Parse(ConfigurationManager.AppSettings["AlwaysHideMenus"]);
            return showMenu;
        }
    }
}