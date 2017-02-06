using LNF;
using LNF.CommonTools;
using LNF.Data;
using LNF.Data.ClientAccountMatrix;
using LNF.Models.Data;
using LNF.PhysicalAccess;
using LNF.Repository;
using LNF.Repository.Billing;
using LNF.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Data.Models
{
    public class zClientModel : ClientBaseModel
    {
        private LNF.Repository.Data.Org org;
        private ClientOrg co;
        private List<SelectListItem> paging;
        private List<ClientListItem> clients;
        private List<AddressItem> addresses;
        private List<ManagerItem> managers;

        public Client Client { get; set; }
        public string Command { get; set; }
        public string CommandArgument { get; set; }
        public string UserName { get; set; }
        public string SearchText { get; set; }
        public string FName { get; set; }
        public string MName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int ClientOrgID { get; set; }
        public int ClientID { get; set; }
        public int OrgID { get; set; }
        public int ManagerClientOrgID { get; set; }
        public int UserClientOrgID { get; set; }
        public int AccountID { get; set; }
        public int DemCitizenID { get; set; }
        public int DemEthnicID { get; set; }
        public int DemRaceID { get; set; }
        public int DemGenderID { get; set; }
        public int DemDisabilityID { get; set; }
        public ClientPrivilege Privs { get; set; }
        public int Communities { get; set; }
        public int TechnicalFieldID { get; set; }
        public int DepartmentID { get; set; }
        public int RoleID { get; set; }
        public bool IsManager { get; set; }
        public bool IsFinManager { get; set; }
        public int SubsidyStartDateYear { get; set; }
        public int SubsidyStartDateMonth { get; set; }
        public int NewFacultyStartDateYear { get; set; }
        public int NewFacultyStartDateMonth { get; set; }
        public int BillingTypeID { get; set; }

        public string SaveErrorMessage { get; set; }

        private Card[] cards;

        private LNF.Repository.Data.Org GetOrg()
        {
            if (HttpContext.Current.Session["OrgID"] == null)
                HttpContext.Current.Session["OrgID"] = OrgUtility.GetPrimaryOrg().OrgID;

            int orgId = Convert.ToInt32(HttpContext.Current.Session["OrgID"]);

            if (org == null || org.OrgID != orgId)
                org = DA.Current.Single<Org>(orgId);

            return org;
        }

        private ClientOrg GetClientOrg()
        {
            if (co == null)
                co = DA.Current.Single<ClientOrg>(ClientOrgID);

            return co;
        }

        public IEnumerable<Client> GetAllClients()
        {
            return DA.Current.Query<Client>();
        }

        public void GetClients()
        {
            clients = new List<ClientListItem>();

            Org current = GetOrg();
            OrgID = current.OrgID;

            if (current != null)
            {
                var drybox = DA.Current.Query<DryBoxClient>().Where(x => x.HasDryBox && x.OrgID == current.OrgID);

                var query = DA.Current.Query<ClientOrg>().Where(x => x.Org == current && x.Active).ToArray().OrderBy(x => x.Client.DisplayName);

                int itemsPerPage = 10;
                paging = new List<SelectListItem>();
                SelectListItem currentItem = null;
                int index = 0;
                for (int x = 0; x < query.Count(); x++)
                {
                    ClientOrg co = query.ElementAt(x);

                    if (x % itemsPerPage == 0)
                    {
                        SelectListItem listItem = new SelectListItem();
                        listItem.Text = co.Client.LName;

                        if (currentItem != null)
                        {
                            currentItem.Text += " - " + query.ElementAt(x - 1).Client.LName;
                            paging.Add(currentItem);
                            index++;
                        }

                        currentItem = listItem;

                        listItem.Value = index.ToString();
                    }

                    ClientListItem item = new ClientListItem()
                    {
                        ClientID = co.Client.ClientID,
                        ClientOrgID = co.ClientOrgID,
                        DisplayName = co.Client.DisplayName,
                        //LName = co.Client.LName,
                        //FName = co.Client.FName,
                        HasDryBox = false,
                        DryBoxAccount = string.Empty
                        //,PageIndex = index
                    };

                    DryBoxClient dbc = drybox.FirstOrDefault(i => i.ClientOrgID == co.ClientOrgID);
                    if (dbc != null)
                    {
                        item.HasDryBox = true;
                        item.DryBoxAccount = dbc.AccountName;
                    }

                    clients.Add(item);
                }

                if (current != null)
                {
                    if (query.Count() > 0)
                    {
                        currentItem.Text += " - " + query.Last().Client.LName;
                        paging.Add(currentItem);
                    }
                }
            }
        }

        public bool IsValidEmail(string text)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(text);
                return addr != null;
            }
            catch
            {
                return false;
            }
        }

        public bool Save()
        {
            SaveErrorMessage = string.Empty;

            if (string.IsNullOrEmpty(UserName))
                SaveErrorMessage += "<div><strong>&times;</strong> Username is required</div>";

            if (string.IsNullOrEmpty(LName))
                SaveErrorMessage += "<div><strong>&times;</strong> Last name is required</div>";

            if (string.IsNullOrEmpty(Email))
                SaveErrorMessage += "<div><strong>&times;</strong> Email is required</div>";
            else
            {
                if (!IsValidEmail(Email))
                    SaveErrorMessage += "<div><strong>&times;</strong> Email is not valid</div>";
            }

            if (!string.IsNullOrEmpty(SaveErrorMessage))
                return false;

            ClientOrg co = null;
            Client c = null;

            if (ClientOrgID > 0)
            {
                co = DA.Current.Single<ClientOrg>(ClientOrgID);
                c = co.Client;

                c.LName = LName;
                c.FName = FName;
                c.MName = MName;
                c.DemCitizenID = DemCitizenID;
                c.DemEthnicID = DemEthnicID;
                c.DemRaceID = DemRaceID;
                c.DemGenderID = DemGenderID;
                c.DemDisabilityID = DemDisabilityID;
                c.Privs = Privs;
                c.Communities = Communities;
                c.TechnicalFieldID = TechnicalFieldID;

                co.Department = DA.Current.Single<Department>(DepartmentID);
                co.Role = DA.Current.Single<Role>(RoleID);
                co.IsManager = IsManager;
                co.IsFinManager = IsFinManager;
                co.Email = Email;
                co.Phone = Phone;
                co.SubsidyStartDate = new DateTime(SubsidyStartDateYear, SubsidyStartDateMonth, 1);
                co.NewFacultyStartDate = new DateTime(NewFacultyStartDateYear, NewFacultyStartDateMonth, 1);

                //c.Save();
                //co.Save();
            }
            else
            {
                c = new Client()
                {
                    UserName = this.UserName,
                    LName = LName,
                    FName = FName,
                    MName = MName,
                    DemCitizenID = DemCitizenID,
                    DemEthnicID = DemEthnicID,
                    DemRaceID = DemRaceID,
                    DemGenderID = DemGenderID,
                    DemDisabilityID = DemDisabilityID,
                    Privs = Privs,
                    Communities = Communities,
                    TechnicalFieldID = TechnicalFieldID,
                };

                c.SetPassword(UserName);

                c.Enable();

                co = new ClientOrg()
                {
                    Client = c,
                    Org = GetOrg(),
                    Department = DA.Current.Single<Department>(DepartmentID),
                    Role = DA.Current.Single<Role>(RoleID),
                    ClientAddressID = 0,
                    Email = this.Email,
                    Phone = this.Phone,
                    IsManager = this.IsManager,
                    IsFinManager = this.IsFinManager,
                    SubsidyStartDate = new DateTime(SubsidyStartDateYear, SubsidyStartDateMonth, 1),
                    NewFacultyStartDate = new DateTime(NewFacultyStartDateYear, NewFacultyStartDateMonth, 1),
                };

                co.Enable();
            }

            if (!string.IsNullOrEmpty(SaveErrorMessage))
                return false;

            //BillingType
            BillingType bt = DA.Current.Single<BillingType>(BillingTypeID);
            ClientOrgBillingTypeLog cobt = DA.Current.Query<ClientOrgBillingTypeLog>().OrderByDescending(x => x.EffDate).FirstOrDefault(x => x.ClientOrg == co);

            if (cobt == null)
            {
                //never had a BillingType before
                DA.Current.SaveOrUpdate(new ClientOrgBillingTypeLog()
                {
                    ClientOrg = co,
                    BillingType = bt,
                    EffDate = DateTime.Now,
                    DisableDate = null
                });
            }
            else
            {
                if (cobt.BillingType.BillingTypeID != BillingTypeID)
                {
                    //existing BillingType that has been modified
                    cobt.DisableDate = DateTime.Now;
                    DA.Current.SaveOrUpdate(cobt);

                    DA.Current.SaveOrUpdate(new ClientOrgBillingTypeLog()
                    {
                        ClientOrg = co,
                        BillingType = bt,
                        EffDate = DateTime.Now,
                        DisableDate = null
                    });
                }
            }

            //Physical Access
            if (c.HasPriv(ClientPrivilege.PhysicalAccess))
            {
                if (Providers.PhysicalAccess.GetBadge(c).Count() == 0)
                {
                    //Providers.PhysicalAccess.AddClient(c);
                }
            }

            if (ClientOrgID > 0)
            {
                ClientUtility.UpdatePhysicalAccess(c);
            }

            return true;
        }

        public Card[] GetCards()
        {
            if (cards == null)
            {
                Badge b = Providers.PhysicalAccess.GetBadge(Client).FirstOrDefault();
                if (b != null)
                    cards = b.GetCards().ToArray();
                else
                    cards = new Card[] { };
            }

            return cards;
        }

        public void GetClient()
        {
            var current = GetClientOrg();
            if (current != null)
            {
                Client = current.Client;
                ClientID = Client.ClientID;
                FName = Client.FName;
                MName = Client.MName;
                LName = Client.LName;
                UserName = Client.UserName;
                DemCitizenID = Client.DemCitizenID;
                DemEthnicID = Client.DemEthnicID;
                DemRaceID = Client.DemRaceID;
                DemGenderID = Client.DemGenderID;
                DemDisabilityID = Client.DemDisabilityID;
                Privs = Client.Privs;
                Communities = Client.Communities;
                TechnicalFieldID = Client.TechnicalFieldID;
                DepartmentID = co.Department.DepartmentID;
                RoleID = co.Role.RoleID;
                Email = co.Email;
                Phone = co.Phone;
                IsManager = co.IsManager;
                IsFinManager = co.IsFinManager;
                OrgID = current.Org.OrgID;
                BillingTypeID = current.GetBillingType().BillingTypeID;
            }
            else
            {
                DemCitizenID = GetDemCitizenItems().FirstOrDefault().ValueOf(x => x.DemCitizenID);
                DemEthnicID = GetDemEthnicItems().FirstOrDefault().ValueOf(x => x.DemEthnicID);
                DemRaceID = GetDemRaceItems().FirstOrDefault().ValueOf(x => x.DemRaceID);
                DemGenderID = GetDemGenderItems().FirstOrDefault().ValueOf(x => x.DemGenderID);
                DemDisabilityID = GetDemDisabilityItems().FirstOrDefault().ValueOf(x => x.DemDisabilityID);
                BillingTypeID = GetBillingTypes().FirstOrDefault().ValueOf(x => x.BillingTypeID);
            }
        }

        public IEnumerable<ClientListItem> GetClientListItems()
        {
            return clients;
        }

        public IEnumerable<SelectListItem> GetPagingSelectItems()
        {
            return paging;
        }

        public IEnumerable<SelectListItem> GetClientSelectItems()
        {
            return DA.Current.Query<Client>().Where(x => x.Active).Select(x => new SelectListItem() { Text = x.DisplayName, Value = x.UserName });
        }

        public IEnumerable<SelectListItem> GetOrgSelectItems()
        {
            IList<SelectListItem> result = OrgUtility.SelectActive().Select(x => new SelectListItem() { Text = x.OrgName, Value = x.OrgID.ToString(), Selected = x.OrgID == GetOrgID() }).ToList();
            SelectListItem selected = result.FirstOrDefault(x => x.Selected);
            return result;
        }

        public IEnumerable<SelectListItem> GetManagerSelectItems()
        {
            List<SelectListItem> result = new List<SelectListItem>();

            if (OrgID == 0)
                return result;

            IList<ClientOrg> query = ClientOrgUtility.SelectOrgManagers(OrgID);

            result.AddRange(query.Select(x => new SelectListItem() { Value = x.ClientOrgID.ToString(), Text = x.Client.DisplayName }).ToList());

            return result;
        }

        public IEnumerable<SelectListItem> GetTechnicalFieldSelectItems()
        {
            return DA.Current.Query<TechnicalField>().Select(x => new SelectListItem() { Text = x.TechnicalFieldName, Value = x.TechnicalFieldID.ToString() });
        }

        public IEnumerable<SelectListItem> GetDepartmentSelectItems()
        {
            var current = GetOrg();
            return DA.Current.Query<Department>().Where(x => x.Org == current).Select(x => new SelectListItem() { Text = x.DepartmentName, Value = x.DepartmentID.ToString() });
        }

        public IEnumerable<SelectListItem> GetRoleSelectItems()
        {
            return DA.Current.Query<Role>().Select(x => new SelectListItem() { Text = x.RoleName, Value = x.RoleID.ToString() });
        }

        public string ManagersSelectControl()
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<SelectListItem> items = GetManagerSelectItems();
            if (items.Count() > 0)
            {
                sb.Append("<select id=\"ManagerClientOrgID\" name=\"ManagerClientOrgID\" class=\"manager-clientorg-id\">");
                sb.Append("<option value=\"0\"> --Select -- </option>");
                foreach (SelectListItem item in items)
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", item.Value, item.Text);
                }
                sb.Append("</select>");
            }
            else
                sb.Append("<span class=\"nodata\">No managers were found.</span>");

            return sb.ToString();
        }

        public IEnumerable<Client> Search()
        {
            if (string.IsNullOrEmpty(SearchText))
                return new List<Client>();

            IList<Client> query = DA.Current.Query<Client>().Where(x => x.UserName.Contains(SearchText) || x.LName.Contains(SearchText) || x.FName.Contains(SearchText) || x.ClientID.ToString() == SearchText).ToList();

            return query;
        }

        public ClientAccount GetClientAccount()
        {
            if (UserClientOrgID != 0 && AccountID != 0)
                return DA.Current.Query<ClientAccount>().FirstOrDefault(x => x.ClientOrg.ClientOrgID == UserClientOrgID && x.Account.AccountID == AccountID);
            else
                return null;
        }

        public bool UpdateClientAccount(out string message)
        {
            message = string.Empty;
            bool result = true;

            if (OrgID == 0)
            {
                message += "<div>&bull; OrgID is missing</div>";
                result = false;
            }

            if (ManagerClientOrgID == 0)
            {
                message += "<div>&bull; ManagerClientOrgID is missing</div>";
                result = false;
            }

            if (UserClientOrgID == 0)
            {
                message += "<div>&bull; UserClientOrgID is missing</div>";
                result = false;
            }

            if (AccountID == 0)
            {
                message += "<div>&bull; AccountID is missing</div>";
                result = false;
            }

            if (string.IsNullOrEmpty(Command))
            {
                message += "<div>&bull; Command is missing</div>";
                result = false;
            }

            if (!result) return result;

            switch (CommandArgument)
            {
                case "add":
                    result = EnableClientAccount(out message);
                    break;
                case "remove":
                    result = DisableClientAccount(out message);
                    break;
                default:
                    message += "Invalid command.";
                    result = false;
                    break;
            }

            return result;
        }

        public bool EnableClientAccount(out string message)
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
            }

            ca.Enable();

            //Here this will always grant Prowatch access
            UpdateClientAccess(ca);

            //A final check...
            if (ca.ClientOrg.ClientOrgID == (int)UserClientOrgID)
            {
                message = string.Empty;
                return true;
            }
            else
            {
                message = string.Format("EnableClientAccount failed. Expected ClientOrgID: {0}, Actual ClientOrgID: {1}", (int)UserClientOrgID, ca.ClientOrg.ClientOrgID);
                return false;
            }
        }

        public bool DisableClientAccount(out string message)
        {
            ClientAccount ca = GetClientAccount();

            if (ca == null)
            {
                message = string.Format("Could not find ClientAccount record for ClientOrgID: {0}", UserClientOrgID);
                return false;
            }

            message = string.Empty;

            ca.Disable();

            //Here this will revoke Prowatch access if there are no more active accounts.
            UpdateClientAccess(ca);

            return true;
        }

        public void UpdateClientAccess(ClientAccount ca)
        {
            var c = ca.ClientOrg.Client;
            var checks = ClientUtility.UpdatePhysicalAccess(c);
            DA.Current.SaveOrUpdate(c);
        }

        public object HandleAjaxCommand()
        {
            switch (Command)
            {
                case "reset-pw":
                    if (!string.IsNullOrEmpty(UserName))
                    {
                        var client = DA.Current.Query<Client>().FirstOrDefault(x => x.UserName == UserName);
                        if (client != null)
                        {
                            client.ResetPassword();
                            return new { Success = true, Message = "" };
                        }
                        else
                            return new { Success = false, Message = string.Format("No client found for username '{0}'", UserName) };
                    }
                    else
                        return new { Success = false, Message = "UserName must not be blank" };
                case "update-managers":
                    return new { Success = true, Message = "", Select = ManagersSelectControl() };
                case "update-matrix":
                    if (ManagerClientOrgID != 0)
                    {
                        Matrix m = new Matrix(ManagerClientOrgID);
                        object matrix = m.MatrixHtml.ToString();
                        object filter = m.FilterHtml.ToString();
                        return new { Success = true, Message = "", Matrix = matrix, Filter = filter };
                    }
                    else
                        return new { Success = false, Message = "ManagerClientOrgID is missing" };
                case "update-client-account":
                    string message;
                    bool success = UpdateClientAccount(out message);
                    return new { Success = success, Message = message };
                default:
                    return new { Success = false, Message = "Invalid command" };
            }
        }

        public string GetOrgName()
        {
            return GetOrg().OrgName;
        }

        public int GetOrgID()
        {
            return GetOrg().OrgID;
        }

        public string GetClientDisplayName()
        {
            var co = GetClientOrg();
            if (co != null)
                return co.Client.DisplayName;
            else
                return string.Empty;
        }

        public IEnumerable<DemCitizen> GetDemCitizenItems()
        {
            return DA.Current.Query<DemCitizen>();
        }

        public IEnumerable<DemEthnic> GetDemEthnicItems()
        {
            return DA.Current.Query<DemEthnic>();
        }

        public IEnumerable<DemRace> GetDemRaceItems()
        {
            return DA.Current.Query<DemRace>();
        }

        public IEnumerable<DemGender> GetDemGenderItems()
        {
            return DA.Current.Query<DemGender>();
        }

        public IEnumerable<DemDisability> GetDemDisabilityItems()
        {
            return DA.Current.Query<DemDisability>();
        }

        public IEnumerable<Priv> GetPrivItems()
        {
            return DA.Current.Query<Priv>();
        }

        public bool HasPriv(Priv p)
        {
            return (Privs & p.PrivFlag) > 0;
        }

        public IEnumerable<Community> GetCommunityItems()
        {
            return DA.Current.Query<Community>();
        }

        public bool HasCommunity(Community c)
        {
            return (Communities & c.CommunityFlag) > 0;
        }

        public IEnumerable<ManagerItem> GetManagerItems()
        {
            if (managers == null)
            {
                ClientOrg current = GetClientOrg();
                managers = DA.Current.Query<ClientManager>().Where(x => x.ClientOrg == current && x.Active).Select(x => ManagerItem.Create(x)).ToList();
            }

            return managers;
        }

        public IEnumerable<AddressItem> GetAddressItems()
        {
            if (addresses == null)
            {
                ClientOrg current = GetClientOrg();
                addresses = DA.Current.Query<Address>().Where(x => x.AddressID == co.ClientAddressID).Select(x => AddressItem.Create("client", x)).ToList();
            }

            return addresses;
        }

        public BillingType[] GetBillingTypes()
        {
            return DA.Current.Query<BillingType>().Where(x => x.IsActive).ToArray();
        }
    }
}