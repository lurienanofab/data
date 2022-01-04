using LNF.Web;
using Microsoft.Owin;
using Owin;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using LNF.Impl.DependencyInjection;

[assembly: OwinStartup(typeof(Data.Startup))]

namespace Data
{
    public class Startup
    {
        public static WebApp WebApp { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            var assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray();

            WebApp = new WebApp();

            var wcc = new WebContainerConfiguration(WebApp.Context);
            wcc.RegisterAllTypes();

            WebApp.BootstrapMvc(assemblies);

            app.UseDataAccess(WebApp.Context);

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);

            app.UseWebApi(config);
        }
    }
}