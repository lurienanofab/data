using Data.Models;
using LNF;
using LNF.CommonTools;
using LNF.Models.Data;
using LNF.Web.Mvc;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Data.Controllers
{
    public class ServiceController : Controller
    {
        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Index(ServiceModel model)
        {
            model.CurrentPage = "Service";
            model.CurrentSubMenuItem = "service";
            return View(model);
        }

        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Scheduler(ServiceModel model)
        {
            model.CurrentPage = "Service";
            model.CurrentSubMenuItem = "service";
            model.InitLog();
            return View(model);
        }

        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult PurgeLog(ServiceModel model)
        {
            if (model.PurgeLogDate != DateTime.MinValue)
                Providers.Log.Current.Purge(model.PurgeLogDate);
            return RedirectToAction("Scheduler");
        }

        [LNFAuthorize]
        public ActionResult SchedulerTask(ServiceModel model)
        {
            Server.ScriptTimeout = 3600;
            return Json(model.HandleCommand(), JsonRequestBehavior.AllowGet);
        }

        [LNFAuthorize(ClientPrivilege.Developer)]
        public ActionResult ReadStoreDataClean(DateTime sd, DateTime ed, int clientId = 0, int itemId = 0, ReadStoreDataManager.StoreDataCleanOption option = ReadStoreDataManager.StoreDataCleanOption.AllItems)
        {
            
            var mgr = new ReadStoreDataManager();
            var dt = mgr.ReadStoreDataClean(option, sd, ed, clientId, itemId);

            var result = dt.AsEnumerable().Select(dr => new
            {
                StoreDataID = dr["StoreDataID"],
                ClientID = dr["ClientID"],
                ItemID = dr["ItemID"],
                OrderDate = dr["OrderDate"],
                AccountID = dr["AccountID"],
                Quantity = dr["Quantity"],
                UnitCost = dr["UnitCost"],
                CategoryID = dr["CategoryID"],
                RechargeItem = dr["RechargeItem"],
                StatusChangeDate = dr["StatusChangeDate"]
            }).ToArray();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [LNFAuthorize(ClientPrivilege.Developer)]
        public ActionResult UpdateDataTables(string types)
        {
            try
            {
                DateTime now = DateTime.Now;
                DataTableManager.Update(types.Split(','));
                return Content(string.Format("Updated {0} in {1} seconds", types, (DateTime.Now - now).TotalSeconds), "text/plain");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString(), "text/plain");
            }
        }
    }
}
