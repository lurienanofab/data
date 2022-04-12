using LNF;
using LNF.Data;
using LNF.Web;
using LNF.Web.Mvc;
using System.Web.Mvc;

namespace Data.Controllers
{
    public abstract class DataController : BaseController
    {
        public DataController(IProvider provider) : base(provider) { }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string returnTo = null;

            if (!string.IsNullOrEmpty(Request.QueryString["ReturnTo"]))
            {
                returnTo = Request.QueryString["ReturnTo"];
                if (returnTo == "unset")
                {
                    returnTo = null;
                }
            }
            else
            {
                var obj = HttpContext.Session["return-to"];
                if (obj != null)
                {
                    returnTo = obj.ToString();
                }
            }

            HttpContext.Session["return-to"] = returnTo;
        }

        public IClient CurrentUser
        {
            get
            {
                IClient result;

                if (HttpContext.Items["CurrentUser"] == null)
                { 
                    result = Provider.Data.Client.GetClient(HttpContext.GetCurrentUserName());
                    HttpContext.Items["CurrentUser"] = result;
                }
                else
                {
                    result = HttpContext.Items["CurrentUser"] as IClient;
                }

                return result;
            }
        }
    }
}