using Data.Models;
using LNF.Models.Data;
using LNF.Web.Mvc;
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

        [Route("utility/activelog/{TableName}/{Record}")]
        public ActionResult ActiveLog(UtilityModel model)
        {
            return View(model);
        }
    }
}
