using Data.Models.Api;
using LNF;
using LNF.Data;
using LNF.Repository;
using LNF.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Data.Models.Admin
{
    public class OrgModel : AdminBaseModel
    {
        private IOrgManager OrgManager { get; }
        private IActiveDataItemManager ActiveDataItemManager { get; }

        public OrgModel()
        {
            // TODO: wire-up constructor injection
            OrgManager = ServiceProvider.Current.Use<IOrgManager>();
            ActiveDataItemManager = ServiceProvider.Current.Use<IActiveDataItemManager>();
        }

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
            IList<OrgType> orgTypes = DA.Current.Query<OrgType>()
                .OrderBy(x => x.OrgTypeName)
                .ToList();

            IList<SelectListItem> result = orgTypes.Select(x => new SelectListItem() { Text = x.OrgTypeName, Value = x.OrgTypeID.ToString() }).ToList();

            result.Insert(0, new SelectListItem() { Text = "-- Select --", Value = "0" });

            return result.ToArray();
        }

        public override void Load()
        {
            message = string.Empty;

            if (OrgID == 0)
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
            message = string.Empty;

            if (string.IsNullOrEmpty(OrgName))
            {
                message += GetAlert("Name is required.");
                errors++;
            }

            Org existingOrg = DA.Current.Query<Org>().Where(x => x.OrgName == OrgName).FirstOrDefault();

            //three possibilities: 1) no org with this name exists, 2) existing is the same as this org, 3) existing is different
            if (existingOrg != null && existingOrg.OrgID != OrgID)
            {
                //there is an existing (and different) org with the same name
                message += GetAlert("This name is already used by an {0} org.", existingOrg.Active ? "active" : "inactive");
                errors++;
            }

            OrgType orgType = null;
            if (OrgTypeID == 0)
            {
                message += GetAlert("Type is required.");
                errors++;
            }
            else
            {
                orgType = DA.Current.Single<OrgType>(OrgTypeID);
                if (orgType == null)
                {
                    message += GetAlert("No record found for OrgTypeID {0}", OrgTypeID);
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
                        message += GetAlert("The primary org must be active.");
                        errors++;
                    }
                }
                else
                {
                    //make sure there is a primary org
                    if (primary == null)
                    {
                        message += GetAlert("There must be at least one primary org.");
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
                    PrimaryOrg = CanEditPrimaryOrg() ? PrimaryOrg : false
                };

                DA.Current.SaveOrUpdate(org); // gets a new OrgID
            }
            else
            {
                org = DA.Current.Single<Org>(OrgID);

                if (org == null)
                {
                    message += GetAlert("No record found for OrgID {0}", OrgID);
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
                    ActiveDataItemManager.Enable(org);
                else
                {
                    ActiveDataItemManager.Disable(org);

                    //need to disable any clients where this was the only active org
                    IList<ClientOrg> clientOrgs = OrgManager.GetClientOrgs(org).Where(x => x.Active).ToList();

                    foreach (ClientOrg co in clientOrgs)
                    {
                        //does this ClientOrg have any other active associations?
                        bool hasAnotherActiveClientOrg = DA.Current.Query<ClientOrg>().Any(x => x.Active && x.Client == co.Client && x.Org != co.Org);
                        if (!hasAnotherActiveClientOrg)
                        {
                            //no other active ClientOrgs so disable the Client record also
                            ActiveDataItemManager.Disable(co.Client);
                        }
                    }
                }
            }

            if (CanEditPrimaryOrg() && PrimaryOrg && primary != null)
            {
                primary.PrimaryOrg = false;
                DA.Current.SaveOrUpdate(primary);
            }

            OrgID = org.OrgID;

            return true;
        }

        public AddressModel[] GetAddresses()
        {
            //always return 3 items, one for each OrgAddressType
            List<AddressModel> list = new List<AddressModel>();
            Org org = DA.Current.Single<Org>(OrgID);
            list.AddRange(GetAddressModels(OrgAddressType.Client, DA.Current.Query<Address>().Where(x => x.AddressID == org.DefClientAddressID).ToArray()));
            list.AddRange(GetAddressModels(OrgAddressType.Billing, DA.Current.Query<Address>().Where(x => x.AddressID == org.DefBillAddressID).ToArray()));
            list.AddRange(GetAddressModels(OrgAddressType.Shipping, DA.Current.Query<Address>().Where(x => x.AddressID == org.DefShipAddressID).ToArray()));
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