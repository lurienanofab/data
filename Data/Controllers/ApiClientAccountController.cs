using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LNF.Data;
using LNF.Repository;
using LNF.Repository.Data;
using Data.Models.Api;

namespace Data.Controllers
{
    public class ApiClientAccountController : ApiController
    {
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
                    ClientAccount ca = DA.Current.Query<ClientAccount>().FirstOrDefault(x => x.ClientOrg.ClientOrgID == id && x.Account.AccountID == model.AccountID);

                    if (ca == null)
                    {
                        //no existing ClientAccount record so create a new one
                        ca = new ClientAccount()
                        {
                            ClientOrg = DA.Current.Single<ClientOrg>(id),
                            Account = DA.Current.Single<Account>(model.AccountID),
                            IsDefault = false,
                            Manager = false
                        };
                    }

                    ca.Enable();

                    //may need to restore physical access because there is now an active acct and other requirements are met
                    ClientUtility.UpdatePhysicalAccess(ca.ClientOrg.Client);

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
                    ClientAccount ca = DA.Current.Query<ClientAccount>().FirstOrDefault(x => x.ClientOrg.ClientOrgID == id && x.Account.AccountID == model.AccountID);
                    if (ca != null)
                    {
                        ca.Disable();

                        //may not have physical access any more if there are no more active accounts
                        ClientUtility.UpdatePhysicalAccess(ca.ClientOrg.Client);

                        return true;
                    }
                    break;
            }

            return result;
        }
    }
}
