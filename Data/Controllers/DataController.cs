using LNF;
using LNF.Data;
using LNF.Web;
using LNF.Web.Mvc;

namespace Data.Controllers
{
    public abstract class DataController : BaseController
    {
        public DataController(IProvider provider) : base(provider) { }

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