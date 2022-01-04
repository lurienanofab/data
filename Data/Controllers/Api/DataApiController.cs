using LNF;
using LNF.DataAccess;
using System.Web.Http;

namespace Data.Controllers.Api
{
    public abstract class DataApiController : ApiController
    {
        protected IProvider Provider { get; }
        protected ISession DataSession => Provider.DataAccess.Session;

        public DataApiController(IProvider provider)
        {
            Provider = provider;
        }
    }
}
