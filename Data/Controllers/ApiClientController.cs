using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LNF.Data;
using LNF.Repository;
using LNF.Repository.Data;

namespace Data.Controllers
{
    public class ApiClientController : ApiController
    {
        public IEnumerable<ClientItem> Get()
        {
            return DA.Current.Query<Client>().Select(x => new ClientItem
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
