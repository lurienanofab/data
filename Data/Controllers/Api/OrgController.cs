using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Data.Controllers.Api
{
    public class OrgController : ApiController
    {
        [Route("api/org/active")]
        public IEnumerable<IOrg> GetActiveOrgs()
        {
            var orgs = DA.Current.Query<Org>().Where(x => x.Active).OrderBy(x => x.OrgName).CreateModels<IOrg>();
            return orgs;
        }
    }
}
