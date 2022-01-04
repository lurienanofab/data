using Data.Controllers;
using Data.Models.Admin;
using LNF.Data;
using LNF.Impl.Repository.Data;
using LNF.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Data.Tests.Admin
{
    [TestClass]
    public class OrgTests : BaseTest
    {
        [TestMethod]
        public void OrgTests_CanGetOrg()
        {
            var org = DataSession.Single<Org>(17);
            Assert.AreEqual("University of Michigan", org.OrgName);
        }

        [TestMethod]
        public void CanSaveOrg()
        {
            IClient currentUser = new LNF.Data.ClientItem
            {
                ClientID = 1301,
                UserName = "jgett",
                LName = "Getty",
                FName = "James"
            };

            var model = new OrgModel()
            {
                CurrentUser = currentUser,
                OrgName = string.Format("Test Org [{0:yyyyMMddHHmmss}]", DateTime.Now),
                OrgTypeID = 2
            };

            var result = model.Save();
            var message = model.GetMessage();

            Assert.IsNull(message);
            Assert.IsTrue(result);

            var alog = DataSession.Query<ActiveLog>().FirstOrDefault(x => x.TableName == "Org" && x.Record == model.OrgID);

            Assert.IsNotNull(alog);
            Assert.AreEqual(alog.EnableDate, DateTime.Now.Date);
            Assert.IsNull(alog.DisableDate);

            Console.WriteLine(model.OrgID);
        }

        [TestMethod]
        public void CanGetAddNewOrgUrl()
        {
            var routes = new RouteCollection();

            routes.MapMvcAttributeRoutes(Assembly.GetAssembly(typeof(HomeController)));

            var helper = routes.CreateMockUrlHelper("/drybox");

            var actionUrl = helper.Action("OrgEdit", "Admin", new { OrgID = 0 });

            string expected = "/drybox/admin/org/edit/0";

            Assert.AreEqual(expected, actionUrl);
        }
    }

    public static class MvcTestExtensions
    {
        //http://stackoverflow.com/questions/20034983/mapmvcattributeroutes-this-method-cannot-be-called-during-the-applications-pre
        public static void MapMvcAttributeRoutes(this RouteCollection routeCollection, Assembly controllerAssembly)
        {
            var controllerTypes = (from type in controllerAssembly.GetExportedTypes()
                                   where
                                       type != null && type.IsPublic
                                       && type.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                                       && !type.IsAbstract && typeof(IController).IsAssignableFrom(type)
                                   select type).ToList();

            var attributeRoutingAssembly = typeof(RouteCollectionAttributeRoutingExtensions).Assembly;
            var attributeRoutingMapperType =
                attributeRoutingAssembly.GetType("System.Web.Mvc.Routing.AttributeRoutingMapper");

            var mapAttributeRoutesMethod = attributeRoutingMapperType.GetMethod(
                "MapAttributeRoutes",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(RouteCollection), typeof(IEnumerable<Type>) },
                null);

            mapAttributeRoutesMethod.Invoke(null, new object[] { routeCollection, controllerTypes });
        }

        public static HttpContextBase CreateMockHttpContext(string applicationPath)
        {
            var httpContext = MockRepository.GenerateStub<HttpContextBase>();
            var httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
            var httpResponse = MockRepository.GenerateStub<HttpResponseBase>();

            httpRequest.Stub(x => x.ApplicationPath).Return(applicationPath);

            httpResponse.Stub(r => r.ApplyAppPathModifier(Arg<string>.Is.Anything)).Return(null)
                        .WhenCalled(x => x.ReturnValue = x.Arguments[0]);

            httpContext.Stub(c => c.Request).Return(httpRequest).Repeat.Any();
            httpContext.Stub(c => c.Response).Return(httpResponse).Repeat.Any();

            return httpContext;
        }

        public static UrlHelper CreateMockUrlHelper(this RouteCollection routeCollection, string applicationPath)
        {
            var httpContext = CreateMockHttpContext(applicationPath);

            var routeData = new RouteData();

            var requestContext = new RequestContext(httpContext, routeData);

            return new UrlHelper(requestContext, routeCollection);
        }

        public static HtmlHelper CreateMockHttpHelper(this RouteCollection routeCollection, string applicationPath)
        {
            var httpContext = CreateMockHttpContext(applicationPath);

            var viewContext = MockRepository.GenerateStub<ViewContext>();

            var routeData = new RouteData();

            viewContext.HttpContext = httpContext;
            viewContext.RequestContext = new RequestContext(httpContext, routeData);
            viewContext.RouteData = routeData;
            viewContext.ViewData = new ViewDataDictionary { Model = null };

            var helper = new HtmlHelper(viewContext, new ViewPage(), routeCollection);

            return helper;
        }
    }
}
