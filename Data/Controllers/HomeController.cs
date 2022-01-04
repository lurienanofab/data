using Data.Models;
using Data.Models.CommandLine;
using LNF;
using LNF.CommonTools;
using LNF.Data;
using LNF.Web.Mvc;
using System;
using System.Web.Mvc;

namespace Data.Controllers
{
    public class HomeController : DataController
    {
        public HomeController(IProvider provider) : base(provider) { }

        [Route("")]
        [LNFAuthorize(ClientPrivilege.Administrator, accessDeniedViewName: null)]
        public ActionResult Index(HomeModel model, string app = null)
        {
            ViewBag.App = app;
            model.Provider = Provider;
            model.CurrentPage = "Home";
            return View(model);
        }

        [Route("dispatch/{name?}")]
        public ActionResult Dispatch(string name = null, string returnTo = null)
        {
            object routeValues = null;

            string action;
            string controller;

            switch (name)
            {
                case "historical-database-report":
                    action = "Index";
                    controller = "Home";
                    break;
                case "password-reset":
                    action = "PasswordReset";
                    controller = "Admin";
                    break;
                case "configure-org":
                    action = "Org";
                    controller = "Admin";
                    break;
                case "assign-accounts":
                    action = "AssignAccounts";
                    controller = "Admin";
                    routeValues = new { OrgID = int.Parse(Request.QueryString["OrgID"]) };
                    break;
                default:
                    Session.Remove("return-to");
                    return RedirectToAction("Index");
            }

            Session["return-to"] = returnTo;

            return RedirectToAction(action, controller, routeValues);
        }

        [Route("return")]
        public ActionResult Return()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["return-to"])))
                return RedirectToAction("Index");
            else
                return Redirect(Convert.ToString(Session["return-to"]));
        }

        [Route("command")]
        [LNFAuthorize(ClientPrivilege.Developer)]
        public ActionResult CommandLine(CommandLineModel model)
        {
            return View(model);
        }

        [Route("command/process")]
        [LNFAuthorize(ClientPrivilege.Developer)]
        public ActionResult CommandLineProcess(string cmd)
        {
            Server.ScriptTimeout = 3600;

            object result;
            if (string.IsNullOrEmpty(cmd))
                result = new { Success = false, Message = "Missing command" };
            else
            {
                var scriptResult = CommandLineUtility.Execute(cmd);
                result = new { scriptResult.Success, scriptResult.Message, scriptResult.Data };
            }

            return Json(result);
        }

        [Route("screensaver/clock/{room}"), AllowAnonymous]
        public ActionResult Clock(ClockModel model, string v = null)
        {
            if (v == "test")
                throw new Exception("test");

            model.ApplyOption();
            return View(model);
        }

        [Route("dashboard"), AllowAnonymous]
        public ActionResult Dashboard(DashboardModel model)
        {
            DateTime fom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            model.CompanyName = SendEmail.CompanyName;
            model.DefaultStartDate = fom.AddMonths(-1);
            model.DefaultEndDate = fom.AddDays(-1);
            return View(model);
        }

        [Route("access-denied"), AllowAnonymous]
        public ActionResult AccessDenied(AccessDeniedModel model)
        {
            return View(model);
        }
    }
}
