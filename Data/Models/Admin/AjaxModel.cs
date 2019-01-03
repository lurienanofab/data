using LNF;
using LNF.Cache;
using LNF.Data;
using LNF.Data.ClientAccountMatrix;
using LNF.Repository;
using LNF.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Models.Admin
{
    public class AjaxModel
    {
        private IClientManager ClientManager { get; }
        private IClientOrgManager ClientOrgManager { get; }
        private IActiveDataItemManager ActiveDataItemManager { get; }

        public AjaxModel()
        {
            // TODO: wire-up constructor injection
            ClientManager = ServiceProvider.Current.Use<IClientManager>();
            ClientOrgManager = ServiceProvider.Current.Use<IClientOrgManager>();
            ActiveDataItemManager = ServiceProvider.Current.Use<IActiveDataItemManager>();
        }

        public string Command { get; set; }
        public string Action { get; set; }
        public string UserName { get; set; }
        public int OrgID { get; set; }
        public int ManagerClientOrgID { get; set; }
        public int UserClientOrgID { get; set; }
        public int AccountID { get; set; }

        public object HandleCommand()
        {
            switch (Command)
            {
                case "password-reset":
                    if (!string.IsNullOrEmpty(UserName))
                    {
                        var client = DA.Current.Query<Client>().FirstOrDefault(x => x.UserName == UserName);
                        if (client != null)
                        {
                            client.ResetPassword();
                            return new { status = "ok" };
                        }
                        else
                            throw new Exception(string.Format("No client found for username '{0}'", UserName));
                    }
                    else
                        throw new Exception("Missing parameter UserName");
                case "get-managers":
                    if (OrgID == 0)
                        throw new Exception("Missing parameter OrgID");

                    var query = ClientOrgManager.SelectOrgManagers(OrgID);

                    return new { status = "ok", managers = query.Select(x => new { x.ClientOrgID, x.Client.DisplayName }).ToArray() };
                case "update-matrix":
                    if (OrgID == 0)
                        throw new Exception("Missing parameter OrgID");

                    if (ManagerClientOrgID == 0)
                        throw new Exception("Missing parameter ManagerClientOrgID");

                    Matrix m = new Matrix(ManagerClientOrgID);
                    string matrix = m.MatrixHtml.ToString();
                    string filter = m.FilterHtml.ToString();

                    return new { status = "ok", matrix, filter };
                case "update-client-account":
                    if (OrgID == 0)
                        throw new Exception("Missing parameter OrgID");

                    if (ManagerClientOrgID == 0)
                        throw new Exception("Missing parameter ManagerClientOrgID");

                    if (UserClientOrgID == 0)
                        throw new Exception("Missing parameter UserClientOrgID");

                    if (AccountID == 0)
                        throw new Exception("Missing parameter AccountID");

                    UpdateClientAccount();

                    return new { status = "ok" };
                default:
                    throw new Exception("Invalid command");
            }
        }

        public IList<ClientOrg> GetManagers(int orgId)
        {
            return ClientOrgManager.SelectOrgManagers(orgId);
        }

        public void UpdateClientAccount()
        {
            switch (Action)
            {
                case "add":
                    EnableClientAccount();
                    break;
                case "remove":
                    DisableClientAccount();
                    break;
                default:
                    throw new Exception("Invalid action.");
            }
        }

        public void EnableClientAccount()
        {
            ClientAccount ca = GetClientAccount();

            if (ca == null)
            {
                //No record exists yet so this must be the first association of this client and account.
                ca = new ClientAccount
                {
                    ClientOrg = DA.Current.Single<ClientOrg>((int)UserClientOrgID),
                    Account = DA.Current.Single<Account>((int)AccountID),
                    Manager = false,
                    IsDefault = false
                };

                // Need to save to get the ClientAccountID.
                DA.Current.SaveOrUpdate(ca);
            }

            ActiveDataItemManager.Enable(ca);

            var c = DA.Current.Single<ClientInfo>(ca.ClientOrg.Client.ClientID).CreateClientItem();
            var check = AccessCheck.Create(c);
            ClientManager.UpdatePhysicalAccess(check, out string alert);

            //A final check...
            if (ca.ClientOrg.ClientOrgID != UserClientOrgID)
                throw new Exception(string.Format("EnableClientAccount failed. Expected ClientOrgID: {0}, Actual ClientOrgID: {1}", UserClientOrgID, ca.ClientOrg.ClientOrgID));
        }

        public string DisableClientAccount()
        {
            ClientAccount ca = GetClientAccount();

            if (ca == null)
                throw new Exception(string.Format("Could not find ClientAccount record for ClientOrgID: {0}", UserClientOrgID));

            ActiveDataItemManager.Disable(ca);

            var c = DA.Current.Single<ClientInfo>(ca.ClientOrg.Client.ClientID).CreateClientItem();
            var check = AccessCheck.Create(c);
            ClientManager.UpdatePhysicalAccess(check, out string alert);

            return alert;
        }

        public ClientAccount GetClientAccount()
        {
            if (UserClientOrgID != 0 && AccountID != 0)
                return DA.Current.Query<ClientAccount>().FirstOrDefault(x => x.ClientOrg.ClientOrgID == UserClientOrgID && x.Account.AccountID == AccountID);
            else
                return null;
        }
    }
}