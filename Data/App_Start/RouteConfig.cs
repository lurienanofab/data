using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Data
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "ServiceSchedulerTask",
                url: "service/scheduler/task/{Task}",
                defaults: new { controller = "Service", action = "SchedulerTask", Task = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ServiceScheduler",
                url: "service/scheduler",
                defaults: new { controller = "Service", action = "Scheduler" }
            );

            routes.MapRoute(
                name: "ServiceUpdateDataTables",
                url: "service/update-data-tables",
                defaults: new { controller = "Service", action = "UpdateDataTables" }
            );

            routes.MapRoute(
                name: "ServiceReadStoreDataClean",
                url: "service/store-data-clean",
                defaults: new { controller = "Service", action = "ReadStoreDataClean" }
            );

            routes.MapRoute(
                name: "ServicePurgeLog",
                url: "service/purge-log",
                defaults: new { controller = "Service", action = "PurgeLog" }
            );

            routes.MapRoute(
                name: "Service",
                url: "service",
                defaults: new { controller = "Service", action = "Index" }
            );

            routes.MapRoute(
                name: "ViewHtml",
                url: "view/{Alias}",
                defaults: new { controller = "Feed", action = "ViewHtml", Alias = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "FeedReportsConfiguration",
                url: "feed/reports/{alias}/cfg",
                defaults: new { controller = "Feed", action = "Configuration", Alias = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "FeedReports",
                url: "feed/reports/{alias}",
                defaults: new { controller = "Feed", action = "Reports", Alias = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Feed",
                url: "feed/{Alias}/{Format}/{Key}",
                defaults: new { controller = "Feed", action = "Index", Alias = UrlParameter.Optional, Format = UrlParameter.Optional, Key = UrlParameter.Optional }
            );
        }
    }
}
