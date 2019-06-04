using Data.Models.Api;
using LNF;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using System.Linq;
using System.Web.Http;

namespace Data.Controllers
{
    public class ApiClientManagerController : ApiController
    {
        private IActiveLogManager ActiveLogManager { get; }

        public ApiClientManagerController()
        {
            //TODO: wire-up constructor injection
            ActiveLogManager = ServiceProvider.Current.Data.ActiveLog;
        }

        public ClientModel[] Get([FromUri] string option, int id)
        {
            switch (option)
            {
                case "current":
                    //id is ClientOrgID
                    return ApiUtility.GetCurrentManagers(new ClientModel() { ClientOrgID = id });
                case "available":
                    //id is ClientOrgID
                    return ApiUtility.GetAvailableManagers(new ClientModel() { ClientOrgID = id });
                case "all":
                    //id is OrgID
                    return ApiUtility.GetAllManagers(new ClientModel() { ClientOrgID = id });
            }

            return new ClientModel[] { };
        }

        public ClientModel Post([FromUri] string option, [FromBody] ClientModel model, int id)
        {
            ClientModel result = null;

            switch (option)
            {
                case "current":
                    ClientOrg co = DA.Current.Single<ClientOrg>(id);
                    ClientOrg mo = DA.Current.Single<ClientOrg>(model.ClientOrgID);

                    if (co != null && mo != null)
                    {
                        var cm = DA.Current.Query<ClientManager>().FirstOrDefault(x => x.ClientOrg == co && x.ManagerOrg == mo);

                        if (cm == null)
                        {
                            //no existing ClientManager record so create a new one
                            cm = new ClientManager()
                            {
                                ClientOrg = co,
                                ManagerOrg = mo
                            };

                            DA.Current.Insert(cm);
                        }

                        ActiveLogManager.Enable("ClientManager", cm.ClientManagerID);

                        result = ApiUtility.CreateClientModel(cm.ManagerOrg.CreateModel<IClient>());
                    }
                    break;
            }

            return result;
        }

        public bool Delete([FromUri] string option, [FromBody] ClientModel model, int id = 0)
        {
            bool result = false;

            switch (option)
            {
                case "current":
                    ClientOrg co = DA.Current.Single<ClientOrg>(id);
                    ClientOrg mo = DA.Current.Single<ClientOrg>(model.ClientOrgID);
                    var cm = DA.Current.Query<LNF.Repository.Data.ClientManager>().FirstOrDefault(x => x.ClientOrg == co && x.ManagerOrg == mo && x.Active);
                    if (cm != null)
                    {
                        ActiveLogManager.Disable("ClientManager", cm.ClientManagerID);

                        //remove account access if no other manager manages the acct
                        ClientModel m = ApiUtility.CreateClientModel(co.CreateModel<IClient>());
                        AccountModel[] currentAccounts = ApiUtility.GetCurrentAccounts(m);
                        ClientModel[] currentManagers = ApiUtility.GetCurrentManagers(m);

                        foreach (AccountModel acct in currentAccounts)
                        {
                            bool hasManagerForAccount = false;
                            foreach (ClientModel mgr in ApiUtility.GetCurrentManagers(acct))
                            {
                                if (currentManagers.Select(x => x.ClientOrgID).Contains(mgr.ClientOrgID))
                                {
                                    hasManagerForAccount = true;
                                    break;
                                }
                            }

                            if (!hasManagerForAccount)
                            {
                                //the client is assigned to an acct but does not have a manager who manages the acct
                                ClientAccount ca = DA.Current.Query<ClientAccount>().FirstOrDefault(x => x.ClientOrg.ClientOrgID == m.ClientOrgID && x.Account.AccountID == acct.AccountID);

                                if (!ca.Manager) //do not deactivate if this client is also the acct manager!
                                    ActiveLogManager.Disable("ClientAccount", ca.ClientAccountID);
                            }
                        }

                        result = true;
                    }
                    break;
            }

            return result;
        }
    }
}
