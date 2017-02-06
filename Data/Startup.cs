using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using LNF.Impl;

[assembly:OwinStartup(typeof(Data.Startup))]

namespace Data
{
    public class Startup : OwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            base.Configuration(app); // this must come before app.UseWebApi or NHibernate won't work
            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }

        public override void ConfigureAuth(IAppBuilder app)
        {
            //use FormsAuthentication
        }

        public override void ConfigureRoutes(RouteCollection routes)
        {
            AreaRegistration.RegisterAllAreas();
            routes.RouteExistingFiles = false;
            RouteConfig.RegisterRoutes(routes);
        }
    }
}