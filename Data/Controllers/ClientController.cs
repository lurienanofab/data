using Data.Models;
using LNF.Data;
using LNF.Models.Data;
using LNF.Web.Mvc;
using System;
using System.Web.Mvc;

namespace Data.Controllers
{
    [LNFAuthorize(ClientPrivilege.Administrator)]
    public class ClientController : Controller
    {
        public ActionResult Index(zClientModel model)
        {
            model.CurrentPage = "Client";
            model.CurrentSubMenuItem = "client";
            //model.App = "Data";
            model.GetClients();
            return View(model);
        }

        public ActionResult ClientEdit(zClientModel model)
        {
            switch (model.Command)
            {
                case "save":
                    model.Save();
                    break;
                case "save-continue":
                    if (model.Save())
                        return RedirectToAction("Index");
                    break;
            }

            model.CurrentPage = "Client";
            model.CurrentSubMenuItem = "client";
            //model.App = "Data";
            model.GetClient();
            return View(model);
        }

        public ActionResult Access(ClientAccessModel model)
        {
            model.CurrentPage = "Client";
            model.CurrentSubMenuItem = "access";

            if (string.IsNullOrEmpty(model.ActiveTab))
            {
                //if (model.App == "sselData")
                //    return RedirectToAction("Access", new { ActiveTab = "badges", app = "sseldata" });
                //else
                return RedirectToAction("Access", new { ActiveTab = "badges" });
            }
            else if (model.ActiveTab == "events")
            {
                if (!model.StartDate.HasValue)
                    model.StartDate = DateTime.Now.Date.AddDays(-7);
                if (!model.EndDate.HasValue)
                    model.EndDate = DateTime.Now.Date;
            }

            return View(model);
        }

        public ActionResult AccountAssignment(zClientModel model)
        {
            model.CurrentPage = "Client";
            model.CurrentSubMenuItem = "account-assignment";

            if (model.OrgID == 0)
                model.OrgID = OrgUtility.GetPrimaryOrg().OrgID;

            return View(model);
        }

        public ActionResult PasswordReset(zClientModel model)
        {
            model.CurrentPage = "Client";
            model.CurrentSubMenuItem = "password-reset";
            return View(model);
        }

        [HttpPost]
        public ActionResult Ajax(zClientModel model)
        {
            return Json(model.HandleAjaxCommand());
        }

        [HttpPost]
        public ActionResult SessionManager(zClientModel model)
        {
            if (model.Command == "SetOrgID")
                Session["OrgID"] = model.OrgID;

            return Json(new { Success = true, Session = new { OrgID = Session["OrgID"] } });
        }
    }
}
