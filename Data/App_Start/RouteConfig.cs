using System.Web.Mvc;
using System.Web.Routing;

namespace Data
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.RouteExistingFiles = false;
            routes.MapMvcAttributeRoutes();
        }
    }
}
