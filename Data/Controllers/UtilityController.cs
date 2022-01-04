using Data.Models;
using LNF;
using LNF.Data;
using LNF.Web;
using LNF.Web.Mvc;
using System.Web.Mvc;

namespace Data.Controllers
{
    [LNFAuthorize(ClientPrivilege.Administrator)]
    public class UtilityController : DataController
    {
        public UtilityController(IProvider provider) : base(provider) { }

        [Route("utility")]
        public ActionResult Index(UtilityModel model)
        {
            model.Provider = Provider;
            model.CurrentUser = HttpContext.CurrentUser(Provider);
            model.CurrentPage = "Utility";
            model.CurrentSubMenuItem = "utility";
            return View(model);
        }

        [Route("utility/control")]
        public ActionResult Control(UtilityModel model)
        {
            model.Provider = Provider;
            model.CurrentUser = HttpContext.CurrentUser(Provider);
            model.CurrentPage = "Utility";
            model.CurrentSubMenuItem = "control";
            return View(model);
        }

        [Route("utility/control/resource")]
        public ActionResult ControlResource(UtilityModel model)
        {
            model.Provider = Provider;
            model.CurrentUser = HttpContext.CurrentUser(Provider);
            model.CurrentPage = "Utility";
            model.CurrentSubMenuItem = "resource";
            return View(model);
        }

        [Route("utility/fees")]
        public ActionResult Fees(UtilityModel model)
        {
            model.Provider = Provider;
            model.CurrentUser = HttpContext.CurrentUser(Provider);
            return View(model);
        }

        [Route("utility/activelog/{TableName?}/{Record?}")]
        public ActionResult ActiveLog(UtilityModel model)
        {
            model.Provider = Provider;
            model.CurrentUser = HttpContext.CurrentUser(Provider);

            if (string.IsNullOrEmpty(model.TableName))
                model.TableName = "client";

            if (model.Record == 0)
                model.Record = CurrentUser.ClientID;

            return View(model);
        }

        [Route("utility/billing-checks")]
        public ActionResult BillingChecks(UtilityModel model)
        {
            model.Provider = Provider;
            model.CurrentUser = HttpContext.CurrentUser(Provider);

            ViewBag.FixAutoEndMessage = string.Empty;

            if (model.Period.HasValue)
            {
                var util = ServiceProvider.Current.Utility;

                int fixAutoEndCount = -1;

                if (model.Command == "fix-all-auto-end-problems")
                {
                    fixAutoEndCount = util.AutoEnd.FixAllAutoEndProblems(model.Period.Value);
                }

                if (model.Command == "fix-auto-end-problem")
                {
                    fixAutoEndCount = util.AutoEnd.FixAutoEndProblem(model.Period.Value, model.ReservationID);
                }

                if (fixAutoEndCount >= 0)
                    ViewBag.FixAutoEndMessage = string.Format("Auto-end problems fixed: {0}", fixAutoEndCount);

                model.AutoEndProblems = util.AutoEnd.GetAutoEndProblems(model.Period.Value);
            }

            return View(model);
        }
    }
}
