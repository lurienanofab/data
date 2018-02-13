using LNF.Repository;
using LNF.Repository.Data;
using Data.Models;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Data.Controllers
{
    public class AjaxController : Controller
    {
        [Route("ajax/account/edit")]
        public ActionResult AccountEdit()
        {
            try
            {
                var acctEdit = GetAccountEdit();
                return Json(acctEdit, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Route("ajax/account/update")]
        public ActionResult AccountUpdate()
        {
            try
            {
                string json = GetRequestBody();

                var jarr = JArray.Parse(json);

                var acctEdit = GetAccountEdit();

                foreach (JObject jobj in jarr)
                {
                    var field = jobj["field"].Value<string>();
                    var value = jobj["value"].Value<string>();

                    AccountEditUtility.SetProperty(acctEdit, field, value);
                }

                return Json(new { update = true });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost, Route("ajax/account/update/address")]
        public ActionResult AccountAddressUpdate()
        {
            try
            {
                var acctEdit = GetAccountEdit();

                var addressId = Convert.ToInt32(Request.Form["address-id"]);
                var addressType = Request.Form["address-type"];
                var attention = Request.Form["attention"];
                var addressLine1 = Request.Form["address-line1"];
                var addressLine2 = Request.Form["address-line2"];
                var city = Request.Form["city"];
                var state = Request.Form["state"];
                var zip = Request.Form["zip"];
                var country = Request.Form["country"];

                if (addressId == -1)
                {
                    AccountEditUtility.AddAddress(acctEdit, addressType, attention, addressLine1, addressLine2, city, state, zip, country);
                }
                else
                {
                    if (acctEdit.Addresses.ContainsKey(addressType))
                    {
                        acctEdit.Addresses[addressType].Attention = attention;
                        acctEdit.Addresses[addressType].AddressLine1 = addressLine1;
                        acctEdit.Addresses[addressType].AddressLine2 = addressLine2;
                        acctEdit.Addresses[addressType].City = city;
                        acctEdit.Addresses[addressType].State = state;
                        acctEdit.Addresses[addressType].Zip = zip;
                        acctEdit.Addresses[addressType].Country = country;
                    }
                    else
                    {
                        // might as well add it
                        AccountEditUtility.AddAddress(acctEdit, addressType, attention, addressLine1, addressLine2, city, state, zip, country);
                    }
                }

                return Json(new { addressId = acctEdit.Addresses[addressType].AddressID });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost, Route("ajax/account/delete/address")]
        public ActionResult AccountAddressDelete()
        {
            try
            {
                var acctEdit = GetAccountEdit();

                var addressId = Convert.ToInt32(Request.Form["address-id"]);
                var addressType = Request.Form["address-type"];

                if (acctEdit.Addresses.ContainsKey(addressType))
                {
                    if (acctEdit.Addresses[addressType].AddressID == addressId)
                        acctEdit.Addresses[addressType] = null;
                    else
                        throw new Exception(string.Format("AddressID mismatch: Attempting to remove {0} address with AddressID = {1}, but existing {0} address has AddressID = {2}", addressType, addressId, acctEdit.Addresses[addressType].AddressID));
                }

                return Json(new { delete = true });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost, Route("ajax/account/delete/manager")]
        public ActionResult AccountManagerDelete()
        {
            try
            {
                var acctEdit = GetAccountEdit();

                int clientOrgId = Convert.ToInt32(Request.Form["client-org-id"]);

                var managers = acctEdit.Managers.ToList();

                var mgr = managers.FirstOrDefault(x => x.ClientOrgID == clientOrgId);

                if (mgr != null)
                {
                    managers.Remove(mgr);
                    acctEdit.Managers = managers.OrderBy(x => x.LName).ThenBy(x => x.FName);
                    var available = AccountEditUtility.GetAvailableManagers(acctEdit);
                    return Json(new { delete = true, managers = acctEdit.Managers, available });
                }
                else
                    return Json(new { delete = false });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost, Route("ajax/account/add/manager")]
        public ActionResult AccountManagerAdd()
        {
            try
            {
                var acctEdit = GetAccountEdit();

                int clientOrgId = Convert.ToInt32(Request.Form["client-org-id"]);

                var managers = acctEdit.Managers.ToList();

                var mgr = DA.Current.Single<ClientOrgInfo>(clientOrgId);

                if (mgr != null)
                {
                    managers.Add(new AccountManagerEdit()
                    {
                        ClientOrgID = mgr.ClientOrgID,
                        LName = mgr.LName,
                        FName = mgr.FName
                    });

                    acctEdit.Managers = managers.OrderBy(x => x.LName).ThenBy(x => x.FName).ToList();
                    var available = AccountEditUtility.GetAvailableManagers(acctEdit);
                    return Json(new { add = true, managers = acctEdit.Managers, available });
                }
                else
                {
                    return Json(new { add = false });
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { error = ex.Message });
            }
        }
        private AccountEdit GetAccountEdit()
        {
            if (Session["AccountEdit"] == null)
                throw new Exception("No account edit found.");

            return (AccountEdit)Session["AccountEdit"];
        }

        private string GetRequestBody()
        {
            string body;

            using (var inputStream = Request.InputStream)
            using (var sr = new StreamReader(inputStream, Encoding.UTF8))
            {
                body = sr.ReadToEnd();
            }

            return body;
        }
    }
}