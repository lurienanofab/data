using Data.Models;
using LNF.Models.Data;
using LNF.Web.Mvc;
using System.Web.Mvc;

namespace Data.Controllers
{
    [LNFAuthorize(ClientPrivilege.Administrator)]
    public class AccountController : Controller
    {
        public ActionResult Index(zAccountModel model)
        {
            model.CurrentPage = "Account";
            model.CurrentSubMenuItem = "account";
            return View(model);
        }
    }
}
