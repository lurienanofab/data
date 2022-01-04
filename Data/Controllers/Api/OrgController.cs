using LNF;
using LNF.Data;
using LNF.Impl;
using LNF.Impl.Repository.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Data.Controllers.Api
{
    public class OrgController : DataApiController
    {
        public OrgController(IProvider provider) : base(provider) { }

        [Route("api/org/active")]
        public IEnumerable<IOrg> GetActiveOrgs()
        {
            var orgs = DataSession.Query<Org>().Where(x => x.Active).OrderBy(x => x.OrgName).CreateModels<IOrg>();
            return orgs;
        }
    }
}
