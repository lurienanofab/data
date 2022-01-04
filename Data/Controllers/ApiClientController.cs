using Data.Controllers.Api;
using LNF;
using LNF.Impl.Repository.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Data.Controllers
{
    [Route("api/client")]
    public class ApiClientController : DataApiController
    {
        public ApiClientController(IProvider provider) : base(provider) { }

        public IEnumerable<ClientItem> Get()
        {
            return DataSession.Query<Client>().Select(x => new ClientItem
            {
                ClientID = x.ClientID,
                DisplayName = x.DisplayName,
                Active = x.Active
            }).OrderBy(x => x.DisplayName);
        }
    }

    public class ClientItem
    {
        public int ClientID { get; set; }
        public string DisplayName { get; set; }
        public bool Active { get; set; }
    }
}
