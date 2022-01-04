using Data.Controllers.Api;
using Data.Models.Api;
using LNF;
using LNF.Impl.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Data.Controllers
{
    [Route("api/org/address")]
    public class ApiOrgAddressController : DataApiController
    {
        public ApiOrgAddressController(IProvider provider) : base(provider) { }

        public AddressModel[] Get(int orgId)
        {
            //always return 3 items, one for each OrgAddressType
            List<AddressModel> list = new List<AddressModel>();
            Org org = DataSession.Single<Org>(orgId);
            list.AddRange(GetModel(OrgAddressType.Client, DataSession.Query<Address>().Where(x => x.AddressID == org.DefClientAddressID).ToArray()));
            list.AddRange(GetModel(OrgAddressType.Billing, DataSession.Query<Address>().Where(x => x.AddressID == org.DefBillAddressID).ToArray()));
            list.AddRange(GetModel(OrgAddressType.Shipping, DataSession.Query<Address>().Where(x => x.AddressID == org.DefShipAddressID).ToArray()));
            return list.OrderBy(x => x.AddressType).ThenBy(x => x.StreetAddress1).ToArray();
        }

        public AddressModel Post([FromBody] AddressModel model, int orgId)
        {
            Address entity;

            if (model.AddressID > 0)
            {
                //update existing
                entity = DataSession.Single<Address>(model.AddressID);
                entity.InternalAddress = model.Attention;
                entity.StrAddress1 = model.StreetAddress1;
                entity.StrAddress2 = model.StreetAddress2;
                entity.City = model.City;
                entity.State = model.State;
                entity.Zip = model.Zip;
                entity.Country = model.Country;
            }
            else
            {
                //add new
                entity = new Address()
                {
                    InternalAddress = model.Attention,
                    StrAddress1 = model.StreetAddress1,
                    StrAddress2 = model.StreetAddress2,
                    City = model.City,
                    State = model.State,
                    Zip = model.Zip,
                    Country = model.Country
                };
            }

            DataSession.SaveOrUpdate(entity);

            OrgAddressType type = (OrgAddressType)Enum.Parse(typeof(OrgAddressType), model.AddressType);
            Org org = DataSession.Single<Org>(orgId);
            switch (type)
            {
                case OrgAddressType.Client:
                    org.DefClientAddressID = entity.AddressID;
                    break;
                case OrgAddressType.Billing:
                    org.DefBillAddressID = entity.AddressID;
                    break;
                case OrgAddressType.Shipping:
                    org.DefShipAddressID = entity.AddressID;
                    break;
            }

            DataSession.SaveOrUpdate(org);

            return GetModel(type, entity);
        }

        public bool Delete(int orgId, int addressId, string addressType)
        {
            var entity = DataSession.Single<Address>(addressId);
            if (entity != null)
            {
                DataSession.Delete(new[] { entity });
                OrgAddressType type = (OrgAddressType)Enum.Parse(typeof(OrgAddressType), addressType);
                Org org = DataSession.Single<Org>(orgId);
                switch (type)
                {
                    case OrgAddressType.Billing:
                        org.DefBillAddressID = 0;
                        break;
                    case OrgAddressType.Client:
                        org.DefClientAddressID = 0;
                        break;
                    case OrgAddressType.Shipping:
                        org.DefShipAddressID = 0;
                        break;
                }

                DataSession.SaveOrUpdate(org);

                return true;
            }
            else
                return false;
        }

        private IEnumerable<AddressModel> GetModel(OrgAddressType type, IEnumerable<Address> entities)
        {
            List<AddressModel> result = new List<AddressModel>();

            if (entities == null || entities.Count() == 0)
                result.Add(GetModel(type, entity: null));
            else
            {
                foreach (Address entity in entities)
                    result.Add(GetModel(type, entity));
            }

            return result;
        }

        private AddressModel GetModel(OrgAddressType type, Address entity)
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
