using Data.Models.Api;
using LNF.Impl.Repository.Data;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Data.Models.Admin
{
    public class OrgModel : AdminBaseModel
    {
        public int OrgTypeID { get; set; }
        public string OrgName { get; set; }
        public bool NNINOrg { get; set; }
        public bool PrimaryOrg { get; set; }

        public bool CanEditPrimaryOrg()
        {
            //Sandrine does not want this option to be visible at this time.
            return false;
        }

        public SelectListItem[] GetOrgTypeSelectItems()
        {
            IList<OrgType> orgTypes = DataSession.Query<OrgType>()
                .OrderBy(x => x.OrgTypeName)
                .ToList();

            IList<SelectListItem> result = orgTypes.Select(x => new SelectListItem() { Text = x.OrgTypeName, Value = x.OrgTypeID.ToString() }).ToList();

            result.Insert(0, new SelectListItem() { Text = "-- Select --", Value = "0" });

            return result.ToArray();
        }

        public override void Load()
        {
            Message = string.Empty;

            if (OrgID == 0)
            {
                Active = true;
                return;
            }

            Org org = DataSession.Single<Org>(OrgID);

            if (org == null)
            {
                Message = GetAlert("Cannot find OrgID {0}", OrgID);
            }
            else
            {
                OrgTypeID = org.OrgType.OrgTypeID;
                OrgName = org.OrgName;
                NNINOrg = org.NNINOrg;
                PrimaryOrg = org.PrimaryOrg;
                Active = org.Active;
            }
        }

        public override bool Save()
        {
            int errors = 0;
            Message = string.Empty;

            if (string.IsNullOrEmpty(OrgName))
            {
                Message += GetAlert("Name is required.");
                errors++;
            }

            Org existingOrg = DataSession.Query<Org>().Where(x => x.OrgName == OrgName).FirstOrDefault();

            //three possibilities: 1) no org with this name exists, 2) existing is the same as this org, 3) existing is different
            if (existingOrg != null && existingOrg.OrgID != OrgID)
            {
                //there is an existing (and different) org with the same name
                Message += GetAlert("This name is already used by an {0} org.", existingOrg.Active ? "active" : "inactive");
                errors++;
            }

            OrgType orgType = null;
            if (OrgTypeID == 0)
            {
                Message += GetAlert("Type is required.");
                errors++;
            }
            else
            {
                orgType = DataSession.Single<OrgType>(OrgTypeID);
                if (orgType == null)
                {
                    Message += GetAlert("No record found for OrgTypeID {0}", OrgTypeID);
                    errors++;
                }
            }

            Org primary = null;
            if (CanEditPrimaryOrg())
            {
                primary = GetPrimaryOrg();
                if (PrimaryOrg)
                {
                    if (!Active)
                    {
                        Message += GetAlert("The primary org must be active.");
                        errors++;
                    }
                }
                else
                {
                    //make sure there is a primary org
                    if (primary == null)
                    {
                        Message += GetAlert("There must be at least one primary org.");
                        errors++;
                    }
                }
            }

            if (errors > 0)
                return false;

            Org org;
            bool originalActive = false;

            if (OrgID == 0)
            {
                //new record
                org = new Org()
                {
                    DefBillAddressID = 0,
                    DefClientAddressID = 0,
                    DefShipAddressID = 0,
                    NNINOrg = NNINOrg,
                    OrgName = OrgName,
                    OrgType = orgType,
                    PrimaryOrg = CanEditPrimaryOrg() && PrimaryOrg
                };

                DataSession.Insert(org); // gets a new OrgID
            }
            else
            {
                org = DataSession.Single<Org>(OrgID);

                if (org == null)
                {
                    Message += GetAlert("No record found for OrgID {0}", OrgID);
                    return false;
                }

                originalActive = org.Active;

                org.NNINOrg = NNINOrg;
                org.OrgName = OrgName;
                org.OrgType = orgType;
                org.PrimaryOrg = CanEditPrimaryOrg() ? PrimaryOrg : org.PrimaryOrg;
            }

            if (originalActive != Active)
            {
                if (Active)
                    Provider.Data.ActiveLog.Enable(org);
                else
                {
                    Provider.Data.ActiveLog.Disable(org);

                    //need to disable any clients where this was the only active org
                    var clientOrgs = Provider.Data.Client.GetClientOrgs(org.OrgID).Where(x => x.ClientActive);

                    foreach (var co in clientOrgs)
                    {
                        //does this ClientOrg have any other active associations?
                        bool hasAnotherActiveClientOrg = DataSession.Query<ClientOrg>().Any(x => x.Active && x.Client.ClientID == co.ClientID && x.Org.OrgID != co.OrgID);
                        if (!hasAnotherActiveClientOrg)
                        {
                            //no other active ClientOrgs so disable the Client record also
                            var c = DataSession.Single<Client>(co.ClientID);
                            Provider.Data.ActiveLog.Disable(c);
                        }
                    }
                }
            }

            if (CanEditPrimaryOrg() && PrimaryOrg && primary != null)
            {
                primary.PrimaryOrg = false;
                DataSession.SaveOrUpdate(primary);
            }

            OrgID = org.OrgID;

            return true;
        }

        public AddressModel[] GetAddresses()
        {
            //always return 3 items, one for each OrgAddressType
            List<AddressModel> list = new List<AddressModel>();
            Org org = DataSession.Single<Org>(OrgID);
            list.AddRange(GetAddressModels(OrgAddressType.Client, DataSession.Query<Address>().Where(x => x.AddressID == org.DefClientAddressID).ToArray()));
            list.AddRange(GetAddressModels(OrgAddressType.Billing, DataSession.Query<Address>().Where(x => x.AddressID == org.DefBillAddressID).ToArray()));
            list.AddRange(GetAddressModels(OrgAddressType.Shipping, DataSession.Query<Address>().Where(x => x.AddressID == org.DefShipAddressID).ToArray()));
            return list.OrderBy(x => x.AddressType).ThenBy(x => x.StreetAddress1).ToArray();
        }

        public SelectListItem[] GetCopyAddressDropdownItems(string addressType)
        {
            switch (addressType)
            {
                case "Billing":
                    return new[] { new SelectListItem() { Text = "Client" }, new SelectListItem() { Text = "Shipping" } };
                case "Client":
                    return new[] { new SelectListItem() { Text = "Billing" }, new SelectListItem() { Text = "Shipping" } };
                case "Shipping":
                    return new[] { new SelectListItem() { Text = "Billing" }, new SelectListItem() { Text = "Client" } };
                default:
                    throw new ArgumentException("addressType");
            }
        }

        private IEnumerable<AddressModel> GetAddressModels(OrgAddressType type, IEnumerable<Address> entities)
        {
            List<AddressModel> result = new List<AddressModel>();

            if (entities == null || entities.Count() == 0)
                result.Add(GetAddressModel(type, entity: null));
            else
            {
                foreach (Address entity in entities)
                    result.Add(GetAddressModel(type, entity));
            }

            return result;
        }

        private AddressModel GetAddressModel(OrgAddressType type, Address entity)
        {
            var result = new AddressModel
            {
                AddressType = Enum.GetName(typeof(OrgAddressType), type)
            };

            if (entity != null)
            {
                result.AddressID = entity.AddressID;
                result.Attention = entity.InternalAddress;
                result.StreetAddress1 = entity.StrAddress1;
                result.StreetAddress2 = entity.StrAddress2;
                result.City = entity.City;
                result.State = entity.State;
                result.Zip = entity.Zip;
                result.Country = entity.Country;
            }

            return result;
        }
    }
}