using Data.Models.Admin;
using LNF;
using LNF.Billing;
using LNF.Data;
using LNF.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Data.Controllers.Admin
{
    public class AdminController : DataController
    {
        public AdminController(IProvider provider) : base(provider) { }

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
        public ActionResult Client(ClientModel model)
        {
            model.Provider = Provider;
            model.BillingTypes = Provider.Billing.BillingType.GetBillingTypes();
            SetOrg(model);
            SetViewInactive(model);
            return View(model);
        }

        [Route("admin/client/edit/{OrgID}/{ClientID}")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult ClientEdit(ClientModel model)
        {
            model.Provider = Provider;
            model.BillingTypes = Provider.Billing.BillingType.GetBillingTypes();

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
        public ActionResult AssignAccounts(ClientModel model)
        {
            model.Provider = Provider;
            model.BillingTypes = Provider.Billing.BillingType.GetBillingTypes();

            if (model.OrgID == 0)
                model.OrgID = Provider.Data.Org.GetPrimaryOrg().OrgID;

            return View(model);
        }

        [Route("admin/password-reset/{UserName?}")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult PasswordReset(ClientModel model)
        {
            model.Provider = Provider;
            model.BillingTypes = Provider.Billing.BillingType.GetBillingTypes();
            return View(model);
        }

        [Route("admin/account")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Account(AccountModel model)
        {
            model.Provider = Provider;
            SetOrg(model);
            SetViewInactive(model);
            return View(model);
        }

        [Route("admin/account/edit/{AccountID}")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult AccountEdit(AccountModel model)
        {
            model.Provider = Provider;
            throw new NotImplementedException();
        }

        [Route("admin/org")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Org(OrgModel model)
        {
            model.Provider = Provider;
            SetViewInactive(model);
            return View(model);
        }

        [Route("admin/org/edit/{OrgID}")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult OrgEdit(OrgModel model)
        {
            model.Provider = Provider;
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
        public ActionResult Room(RoomModel model)
        {
            model.Provider = Provider;
            SetViewInactive(model);
            return View(model);
        }

        [Route("admin/room/edit/{RoomID}")]
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult RoomEdit(RoomModel model)
        {
            model.Provider = Provider;
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
            model.Provider = Provider;
            return Json(model.HandleCommand());
        }

        private void SetViewInactive(AdminBaseModel model)
        {
            if (bool.TryParse(Request.QueryString["inactive"], out bool result))
                model.ViewInactive = result;
            else
                model.ViewInactive = false;
        }

        private void SetOrg(AdminBaseModel model)
        {
            if (model.OrgID == 0)
            {
                if (int.TryParse(Request.QueryString["OrgID"], out int orgId) && orgId > 0)
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