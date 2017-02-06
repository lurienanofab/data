using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Data
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ApiSubsidy",
                routeTemplate: "api/subsidy",
                defaults: new { controller = "ApiSubsidy" }
            );

            config.Routes.MapHttpRoute(
                name: "ApiActiveLog",
                routeTemplate: "api/activelog",
                defaults: new { controller = "ApiActiveLog" }
            );

            config.Routes.MapHttpRoute(
                name: "ApiOrgAddress",
                routeTemplate: "api/org/address",
                defaults: new { controller = "ApiOrgAddress" }
            );

            config.Routes.MapHttpRoute(
                name: "ApiOrgDepartment",
                routeTemplate: "api/org/department",
                defaults: new { controller = "ApiOrgDepartment" }
            );

            config.Routes.MapHttpRoute(
                name: "ApiNews",
                routeTemplate: "api/news/{option}",
                defaults: new { controller = "ApiNews", option = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ApiClient",
                routeTemplate: "api/client",
                defaults: new { controller = "ApiClient" }
            );

            config.Routes.MapHttpRoute(
                name: "ApiClientManager",
                routeTemplate: "api/client/manager/{option}",
                defaults: new { controller = "ApiClientManager", option = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ApiClientAddress",
                routeTemplate: "api/client/address/{option}",
                defaults: new { controller = "ApiClientAddress", option = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ApiClientAccount",
                routeTemplate: "api/client/account/{option}",
                defaults: new { controller = "ApiClientAccount", option = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ApiClientAccessCheck",
                routeTemplate: "api/client/access/check",
                defaults: new { controller = "ApiClientAccess", action = "Check" }
            );

            config.Routes.MapHttpRoute(
                name: "ApiClientAccess",
                routeTemplate: "api/client/access/inlab",
                defaults: new { controller = "ApiClientAccess", action = "GetInLab" }
            );

            config.Routes.MapHttpRoute(
                name: "ApiClientAccessSocket",
                routeTemplate: "api/client/access/inlab/socket",
                defaults: new { controller = "ApiClientAccess", action = "GetInLabSocket" }
            );

            //config.Routes.MapHttpRoute(
            //    name: "ApiBillingTool",
            //    routeTemplate: "api/billing/tool",
            //    defaults: new { controller = "ApiBilling", action = "GetToolBilling" }
            //);

            //config.Routes.MapHttpRoute(
            //    name: "ApiBilling",
            //    routeTemplate: "api/billing",
            //    defaults: new { controller = "ApiBilling" }
            //);

            config.Routes.MapHttpRoute(
                name: "ApiSchedulerActiveReservations",
                routeTemplate: "api/scheduler/active-reservations",
                defaults: new { controller = "ApiScheduler", action = "GetActiveReservations" }
            );

            config.Routes.MapHttpRoute(
                name: "ApiSchedulerActiveReservationsSocket",
                routeTemplate: "api/scheduler/active-reservations/socket",
                defaults: new { controller = "ApiScheduler", action = "GetActiveReservationsSocket" }
            );

            config.Routes.MapHttpRoute(
                name: "ApiClock",
                routeTemplate: "api/clock/{source}",
                defaults: new { controller = "ApiClock", action = "Get", source = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ApiClockSocket",
                routeTemplate: "api/clock/{source}/socket",
                defaults: new { controller = "ApiClock", action = "GetSocket", source = RouteParameter.Optional }
            );
        }
    }
}
