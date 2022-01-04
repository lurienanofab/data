using LNF.Data;
using LNF.Data.ClientAccountMatrix;
using LNF.Impl.Repository.Data;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Data.Models.Admin
{
    public class AjaxModel : DataModel
    {
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
                        var client = DataSession.Query<Client>().FirstOrDefault(x => x.UserName == UserName);
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

                    var query = Provider.Data.Client.SelectOrgManagers(OrgID);

                    return new { status = "ok", managers = query.Select(x => new { x.ClientOrgID, x.DisplayName }).ToArray() };
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

        public IEnumerable<IClient> GetManagers(int orgId)
        {
            return Provider.Data.Client.SelectOrgManagers(orgId);
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

        public string EnableClientAccount()
        {
            ClientAccount ca = GetClientAccount();

            if (ca == null)
            {
                //No record exists yet so this must be the first association of this client and account.
                ca = new ClientAccount
                {
                    ClientOrg = DataSession.Single<ClientOrg>((int)UserClientOrgID),
                    Account = DataSession.Single<Account>((int)AccountID),
                    Manager = false,
                    IsDefault = false
                };

                // Need to save to get the ClientAccountID.
                DataSession.Insert(ca);
            }

            Provider.Data.ActiveLog.Enable(ca);

            IClient c = DataSession.Single<ClientInfo>(ca.ClientOrg.Client.ClientID);
            Provider.Data.Client.UpdatePhysicalAccess(c, out string alert);

            //A final check...
            if (ca.ClientOrg.ClientOrgID != UserClientOrgID)
                throw new Exception(string.Format("EnableClientAccount failed. Expected ClientOrgID: {0}, Actual ClientOrgID: {1}", UserClientOrgID, ca.ClientOrg.ClientOrgID));

            return alert;
        }

        public string DisableClientAccount()
        {
            ClientAccount ca = GetClientAccount();

            if (ca == null)
                throw new Exception($"Could not find ClientAccount record for ClientOrgID: {UserClientOrgID}");

            Provider.Data.ActiveLog.Disable(ca);

            IClient c = DataSession.Single<ClientInfo>(ca.ClientOrg.Client.ClientID);
            Provider.Data.Client.UpdatePhysicalAccess(c, out string alert);

            return alert;
        }

        public ClientAccount GetClientAccount()
        {
            if (UserClientOrgID != 0 && AccountID != 0)
                return DataSession.Query<ClientAccount>().FirstOrDefault(x => x.ClientOrg.ClientOrgID == UserClientOrgID && x.Account.AccountID == AccountID);
            else
                return null;
        }
    }
}