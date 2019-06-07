using Data.Models.Admin;
using LNF;
using LNF.Models.Data;
using LNF.Web.Mvc;
using System;
using System.Web.Mvc;

namespace Data.Controllers.Admin
{
    public class AdminController : BaseController
    {
        private IOrgManager OrgManager { get; }

        public AdminController()
        {
            // TODO: wire-up constructor injection
            OrgManager = ServiceProvider.Current.Data.Org;
        }

        [Route("admin")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Index()
        {
            if (!string.IsNullOrEmpty(Convert.ToString(Session["return-to"])))
                return RedirectToAction("Return", "Home");
            else
                return RedirectToAction("Client");
        }

        [Route("admin/client")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Client(Models.Admin.ClientModel model)
        {
            SetOrg(model);
            SetViewInactive(model);
            return View(model);
        }

        [Route("admin/client/edit/{OrgID}/{ClientID}")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult ClientEdit(Models.Admin.ClientModel model)
        {
            if (model.Command == "save")
            {
                bool adding = model.ClientID == 0;

                if (model.Save())
                {
                    //when ClientID is zero (adding a new Org) the page should reload so we can add accounts, etc.
                    if (adding)
                        return RedirectToAction("ClientEdit", new { model.OrgID, model.ClientID });
                    else
                        return RedirectToAction("Client");
                }
            }
            else
            {
                model.Load();
            }

            return View(model);
        }

        [Route("admin/assign-accounts/{OrgID?}")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult AssignAccounts(Models.Admin.ClientModel model)
        {
            if (model.OrgID == 0)
                model.OrgID = OrgManager.GetPrimaryOrg().OrgID;
            return View(model);
        }

        [Route("admin/password-reset/{UserName?}")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult PasswordReset(Models.Admin.ClientModel model)
        {
            return View(model);
        }

        [Route("admin/account")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Account(Models.Admin.AccountModel model)
        {
            SetOrg(model);
            SetViewInactive(model);
            return View(model);
        }

        [Route("admin/account/edit/{AccountID}")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult AccountEdit(Models.Admin.AccountModel model)
        {
            throw new NotImplementedException();
        }

        [Route("admin/org")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Org(Models.Admin.OrgModel model)
        {
            SetViewInactive(model);
            return View(model);
        }

        [Route("admin/org/edit/{OrgID}")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult OrgEdit(Models.Admin.OrgModel model)
        {
            if (model.Command == "save")
            {
                bool adding  = false;
                if (model.OrgID == 0)
                {
                    adding = true;
                    model.Active = true; //new orgs are always active
                }

                if (model.Save())
                {
                    //when OrgID is zero (adding a new Org) the page should reload so we can add addresses and departments
                    if (adding)
                        return RedirectToAction("OrgEdit", new { model.OrgID });
                    else
                        return RedirectToAction("Org");
                }
            }
            else
            {
                model.Load();
            }

            return View(model);
        }

        [Route("admin/room")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Room(Models.Admin.RoomModel model)
        {
            SetViewInactive(model);
            return View(model);
        }

        [Route("admin/room/edit/{RoomID}")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult RoomEdit(RoomModel model)
        {
            if (model.Command == "save")
            {
                if (model.SaveRoom())
                    return RedirectToAction("Room");
            }
            else
            {
                model.LoadRoom();
            }

            return View(model);
        }

        [Route("admin/ajax")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Ajax(AjaxModel model)
        {
            return Json(model.HandleCommand());
        }

        private void SetViewInactive(AdminBaseModel model)
        {
            bool result;
            if (bool.TryParse(Request.QueryString["inactive"], out result))
                model.ViewInactive = result;
            else
                model.ViewInactive = false;
        }

        private void SetOrg(AdminBaseModel model)
        {
            if (model.OrgID == 0)
            {
                int orgId;
                if (int.TryParse(Request.QueryString["OrgID"], out orgId) && orgId > 0)
                    model.OrgID = orgId;
                else
                    model.OrgID = model.GetPrimaryOrg().OrgID;
            }
            else
            {
                Session["OrgID"] = model.OrgID;
            }
        }
    }
}