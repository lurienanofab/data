using LNF;
using LNF.Impl.Context;
using LNF.Impl.DependencyInjection.Web;
using LNF.Web;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: OwinStartup(typeof(Data.Startup))]

namespace Data
{
    public class Startup : OwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            var ctx = new WebContext(new WebContextFactory());
            var ioc = new IOC(ctx);
            ServiceProvider.Current = ioc.Resolver.GetInstance<ServiceProvider>();
            base.Configuration(app); // this must come before app.UseWebApi or NHibernate won't work
            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }

        public override void ConfigureAuth(IAppBuilder app)
        {
            //use FormsAuthentication
        }

        public override void ConfigureFilters(GlobalFilterCollection filters)
        {
            FilterConfig.RegisterGlobalFilters(filters);
        }

        public override void ConfigureRoutes(RouteCollection routes)
        {
            AreaRegistration.RegisterAllAreas();
            routes.RouteExistingFiles = false;
            RouteConfig.RegisterRoutes(routes);
        }
    }
}