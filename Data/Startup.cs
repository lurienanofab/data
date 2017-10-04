﻿using LNF.Impl;
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