﻿using Data.Controllers.Api;
using Data.Models.Api;
using LNF;
using LNF.Data;
using LNF.Impl.Repository.Data;
using LNF.Repository;
using System.Linq;
using System.Web.Http;

namespace Data.Controllers
{
    [Route("api/client/account/{option}")]
    public class ApiClientAccountController : DataApiController
    {
        public ApiClientAccountController(IProvider provider) : base(provider) { }

        public AccountModel[] Get([FromUri] string option, int id)
        {
            switch (option)
            {
                case "current":
                    //id is ClientOrgID
                    return ApiUtility.GetCurrentAccounts(new ClientModel() { ClientOrgID = id });
                case "available":
                    //id is ClientOrgID
                    return ApiUtility.GetAvailableAccounts(new ClientModel() { ClientOrgID = id });
                case "all":
                    //id is OrgID
                    return ApiUtility.GetAllAccounts(new ClientModel() { ClientOrgID = id });
            }

            return new AccountModel[] { };
        }

        public AccountModel Post([FromUri] string option, [FromBody] AccountModel model, int id)
        {
            AccountModel result = null;

            switch (option)
            {
                case "current":
                    var ca = DataSession.Query<ClientAccount>().FirstOrDefault(x => x.ClientOrg.ClientOrgID == id && x.Account.AccountID == model.AccountID);

                    if (ca == null)
                    {
                        //no existing ClientAccount record so create a new one
                        ca = new ClientAccount()
                        {
                            ClientOrg = DataSession.Single<ClientOrg>(id),
                            Account = DataSession.Single<Account>(model.AccountID),
                            IsDefault = false,
                            Manager = false
                        };

                        DataSession.Insert(ca);
                    }

                    Provider.Data.ActiveLog.Enable(ca);

                    //may need to restore physical access because there is now an active acct and other requirements are met
                    string alert;
                    IClient c = DataSession.Single<ClientInfo>(ca.ClientOrg.Client.ClientID);
                    Provider.Data.Client.UpdatePhysicalAccess(c, out alert);

                    result = ApiUtility.CreateAccountModel(ca.Account);

                    break;
            }

            return result;
        }

        public bool Delete([FromUri] string option, [FromBody] AccountModel model, int id = 0)
        {
            bool result = false;

            switch (option)
            {
                case "current":
                    ClientAccount ca = DataSession.Query<ClientAccount>().FirstOrDefault(x => x.ClientOrg.ClientOrgID == id && x.Account.AccountID == model.AccountID);
                    if (ca != null)
                    {
                        Provider.Data.ActiveLog.Disable(ca);

                        //may not have physical access any more if there are no more active accounts
                        IClient c = DataSession.Single<ClientInfo>(ca.ClientOrg.Client.ClientID);
                        Provider.Data.Client.UpdatePhysicalAccess(c, out string alert);

                        return true;
                    }
                    break;
            }

            return result;
        }
    }
}
