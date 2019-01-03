using Data.Models;
using LNF.Cache;
using LNF.Models.Data;
using LNF.Web.Mvc;
using OnlineServices.Api.Data;
using System.Web.Mvc;

namespace Data.Controllers
{
    [LNFAuthorize(ClientPrivilege.Administrator)]
    public class UtilityController : BaseController
    {
        [Route("utility")]
        public ActionResult Index(UtilityModel model)
        {
            model.CurrentPage = "Utility";
            model.CurrentSubMenuItem = "utility";
            return View(model);
        }

        [Route("utility/control")]
        public ActionResult Control(UtilityModel model)
        {
            model.CurrentPage = "Utility";
            model.CurrentSubMenuItem = "control";
            return View(model);
        }

        [Route("utility/control/resource")]
        public ActionResult ControlResource(UtilityModel model)
        {
            model.CurrentPage = "Utility";
            model.CurrentSubMenuItem = "resource";
            return View(model);
        }

        [Route("utility/fees")]
        public ActionResult Fees(UtilityModel model)
        {
            return View(model);
        }

        [Route("utility/activelog/{TableName?}/{Record?}")]
        public ActionResult ActiveLog(UtilityModel model)
        {
            if (string.IsNullOrEmpty(model.TableName))
                model.TableName = "client";

            if (model.Record == 0)
                model.Record = CacheManager.Current.CurrentUser.ClientID;

            return View(model);
        }

        [Route("utility/billing-checks")]
        public ActionResult BillingChecks(UtilityModel model)
        {
            ViewBag.FixAutoEndMessage = string.Empty;

            if (model.Period.HasValue)
            {
                var dc = new DataClient();

                int fixAutoEndCount = -1;

                if (model.Command == "fix-all-auto-end-problems")
                {
                    fixAutoEndCount = dc.FixAllAutoEndProblems(model.Period.Value);
                }

                if (model.Command == "fix-auto-end-problem")
                {
                    fixAutoEndCount = dc.FixAutoEndProblem(model.Period.Value, model.ReservationID);
                }

                if (fixAutoEndCount >= 0)
                    ViewBag.FixAutoEndMessage = string.Format("Auto-end problems fixed: {0}", fixAutoEndCount);

                model.AutoEndProblems = dc.GetAutoEndProblems(model.Period.Value);
            }

            return View(model);
        }
    }
}
