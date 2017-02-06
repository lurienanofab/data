using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using LNF;
using LNF.Data;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Web.Mvc.UI;

namespace Data.Models.Admin
{
    public enum DemographicType
    {
        Citizen = 1,
        Gender = 2,
        Race = 3,
        Ethnic = 4,
        Disability = 5
    }

    public class ClientModel : AdminBaseModel
    {
        //Client
        public int ClientID { get; set; }
        public string UserName { get; set; }
        public string LName { get; set; }
        public string FName { get; set; }
        public string MName { get; set; }
        public int PrivFlag { get; set; }
        public int Communities { get; set; }
        public int TechnicalFieldID { get; set; }
        public int DemCitizenID { get; set; }
        public int DemGenderID { get; set; }
        public int DemRaceID { get; set; }
        public int DemEthnicID { get; set; }
        public int DemDisabilityID { get; set; }

        //ClientOrg
        public int ClientOrgID { get; set; }
        public int DepartmentID { get; set; }
        public int RoleID { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsManager { get; set; }
        public bool IsFinManager { get; set; }
        public DateTime? SubsidyStartDate { get; set; }
        public DateTime? NewFacultyStartDate { get; set; }

        //ClientAccount
        public int ManagerClientOrgID { get; set; }

        public SelectListItem[] GetDepartmentSelectItems()
        {
            string path = "";
            System.Xml.Linq.XDocument Output = new System.Xml.Linq.XDocument();

            using (XmlTextWriter writer = (XmlTextWriter)XmlWriter.Create(path, new XmlWriterSettings() { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment }))
            {
                writer.Formatting = Formatting.Indented;
                Output.WriteTo(writer);
                writer.Flush();
                writer.Close();
            }

            return GetDepartments().Select(x => new SelectListItem() { Text = x.DepartmentName, Value = x.DepartmentID.ToString() }).ToArray();
        }

        public IEnumerable<SelectListItem> GetRoleSelectItems()
        {
            return DA.Current.Query<Role>().Select(x => new SelectListItem() { Text = x.RoleName, Value = x.RoleID.ToString() });
        }

        public ClientListItem[] GetClients()
        {
            IQueryable<DryBoxClient> query = null;

            if (ViewInactive)
                query = DA.Current.Query<DryBoxClient>().Where(x => x.OrgID == OrgID);
            else
                query = DA.Current.Query<DryBoxClient>().Where(x => x.OrgID == OrgID && x.ClientOrgActive);

            //ToArray is needed because of GetDisplayName
            var group = query.ToArray().GroupBy(x => new
            {
                ClientID = x.ClientID,
                ClientOrgID = x.ClientOrgID,
                OrgID = x.OrgID,
                OrgManager = x.OrgManager,
                OrgFinManager = x.OrgFinManager,
                ClientOrgActive = x.ClientOrgActive,
                DisplayName = x.GetDisplayName()
            });

            var result = group.Select(g => new ClientListItem()
            {
                ClientID = g.Key.ClientID,
                ClientOrgID = g.Key.ClientOrgID,
                OrgID = g.Key.OrgID,
                DisplayName = g.Key.DisplayName,
                OrgManager = g.Key.OrgManager,
                OrgFinManager = g.Key.OrgFinManager,
                Active = g.Key.ClientOrgActive,
                DryBoxAccount = GetDryBoxAccountName(g.FirstOrDefault(n => n.ClientAccountActive && n.HasDryBox)),
                HasDryBox = g.Count(n => n.ClientAccountActive && n.HasDryBox) > 0
            }).ToArray();

            return result;
        }

        private string GetDryBoxAccountName(DryBoxClient dbc)
        {
            if (dbc == null)
                return string.Empty;
            else
                return dbc.AccountName;
        }

        public string GetDisplayName()
        {
            if (ClientOrgID == 0)
                return string.Empty;
            else
                return string.Format("{0}, {1}", LName, FName);
        }

        public string GetOrgName()
        {
            if (ClientOrgID == 0)
                return string.Empty;
            else
                return DA.Current.Single<Org>(OrgID).OrgName;
        }

        public SelectListItem[] GetTechnicalFieldSelectItems()
        {
            var query = DA.Current.Query<TechnicalField>().ToArray();
            return query.Select(x => new SelectListItem() { Text = x.TechnicalFieldName, Value = x.TechnicalFieldID.ToString() }).ToArray();
        }

        public CheckBoxListItem[] GetPrivCheckBoxListItems()
        {
            var query = DA.Current.Query<Priv>();
            return query.Select(x => new CheckBoxListItem(Convert.ToInt32(x.PrivFlag).ToString(), x.PrivType)).ToArray();
        }

        public CheckBoxListItem[] GetCommunityCheckBoxListItems()
        {
            var query = DA.Current.Query<Community>();
            return query.Select(x => new CheckBoxListItem(x.CommunityFlag.ToString(), x.CommunityName)).ToArray();
        }

        public SelectListItem[] GetDemographicSelectListItems(DemographicType type)
        {
            switch (type)
            {
                case DemographicType.Citizen:
                    return DA.Current.Query<DemCitizen>().Select(x => new SelectListItem() { Text = x.DemCitizenValue, Value = x.DemCitizenID.ToString() }).ToArray();
                case DemographicType.Gender:
                    return DA.Current.Query<DemGender>().Select(x => new SelectListItem() { Text = x.DemGenderValue, Value = x.DemGenderID.ToString() }).ToArray();
                case DemographicType.Race:
                    return DA.Current.Query<DemRace>().Select(x => new SelectListItem() { Text = x.DemRaceValue, Value = x.DemRaceID.ToString() }).ToArray();
                case DemographicType.Ethnic:
                    return DA.Current.Query<DemEthnic>().Select(x => new SelectListItem() { Text = x.DemEthnicValue, Value = x.DemEthnicID.ToString() }).ToArray();
                case DemographicType.Disability:
                    return DA.Current.Query<DemDisability>().Select(x => new SelectListItem() { Text = x.DemDisabilityValue, Value = x.DemDisabilityID.ToString() }).ToArray();
                default:
                    throw new ArgumentException("type");
            }
        }

        public override void Load()
        {
            message = string.Empty;

            if (ClientID == 0)
            {
                Active = true;
                return;
            }

            Org org = DA.Current.Single<Org>(OrgID);

            if (org == null)
            {
                message = GetAlert("Cannot find OrgID {0}", OrgID);
            }
            else
            {
                Client c = DA.Current.Single<Client>(ClientID);
                if (c == null)
                {
                    Active = true;
                    return;
                }
                else
                {
                    UserName = c.UserName;
                    LName = c.LName;
                    FName = c.FName;
                    MName = c.MName;
                    PrivFlag = Convert.ToInt32(c.Privs);
                    Communities = c.Communities;
                    TechnicalFieldID = c.TechnicalFieldID;
                    DemCitizenID = c.DemCitizenID;
                    DemGenderID = c.DemGenderID;
                    DemRaceID = c.DemRaceID;
                    DemEthnicID = c.DemEthnicID;
                    DemDisabilityID = c.DemDisabilityID;
                    Active = c.Active;

                    ClientOrg co = DA.Current.Query<ClientOrg>().FirstOrDefault(x => x.Client.ClientID == ClientID && x.Org.OrgID == OrgID);
                    if (co != null)
                    {
                        ClientOrgID = co.ClientOrgID;
                        DepartmentID = co.Department.DepartmentID;
                        RoleID = co.Role.RoleID;
                        Phone = co.Phone;
                        Email = co.Email;
                        IsManager = co.IsManager;
                        IsFinManager = co.IsFinManager;
                        SubsidyStartDate = co.SubsidyStartDate;
                        NewFacultyStartDate = co.NewFacultyStartDate;
                    }
                }
            }
        }

        public IEnumerable<SelectListItem> GetClientSelectItems()
        {
            return DA.Current.Query<Client>()
                .Where(x => x.Active)
                .ToArray()
                .OrderBy(x => x.DisplayName)
                .Select(x => new SelectListItem() { Text = x.DisplayName, Value = x.UserName })
                .ToArray();
        }

        public override bool Save()
        {
            throw new NotImplementedException();
        }
    }

    public class HtmlWriter : XmlTextWriter
    {
        public HtmlWriter(string path)
            : base(path, System.Text.Encoding.UTF8)
        {

        }

        public override void WriteStartDocument()
        {
            //do nothing
        }
    }
}