using Data.Controllers.Api;
using Data.Models.Api;
using LNF;
using LNF.Impl.Repository.Data;
using LNF.Repository;
using System.Web.Http;

namespace Data.Controllers
{
    [Route("api/client/address/{option}")]
    public class ApiClientAddressController : DataApiController
    {
        public ApiClientAddressController(IProvider provider) : base(provider) { }

        public AddressModel[] Get([FromUri] string option, int id)
        {
            switch (option)
            {
                case "current":
                    return ApiUtility.GetCurrentAddresses("client", id);
            }

            return new AddressModel[] { };
        }

        public AddressModel Post([FromBody] AddressModel model, int id = 0)
        {
            Address addr = DataSession.Single<Address>(model.AddressID);
            AddressModel result = null;

            if (addr != null)
            {
                addr.City = model.City;
                addr.Country = model.Country;
                addr.InternalAddress = model.Attention;
                addr.State = model.State;
                addr.StrAddress1 = model.StreetAddress1;
                addr.StrAddress2 = model.StreetAddress2;
                addr.Zip = model.Zip;
                DataSession.SaveOrUpdate(addr);
                result = ApiUtility.CreateAddressModel("client", addr);
            }
            else
            {
                //add a new address to this ClientOrg
                ClientOrg co = DataSession.Single<ClientOrg>(id);
                if (co != null)
                {
                    addr = new Address()
                    {
                        City = model.City,
                        Country = model.Country,
                        InternalAddress = model.Attention,
                        State = model.State,
                        StrAddress1 = model.StreetAddress1,
                        StrAddress2 = model.StreetAddress2,
                        Zip = model.Zip
                    };

                    DataSession.SaveOrUpdate(addr);

                    co.ClientAddressID = addr.AddressID;

                    DataSession.SaveOrUpdate(co);

                    result = ApiUtility.CreateAddressModel("client", addr);
                }
            }

            return result;
        }

        public bool Delete([FromBody] AddressModel model, int id)
        {
            Address addr = DataSession.Single<Address>(model.AddressID);

            ClientOrg co = DataSession.Single<ClientOrg>(id);

            if (addr != null && co != null)
            {
                DataSession.Delete(addr);
                co.ClientAddressID = 0;
                DataSession.SaveOrUpdate(co);

                return true;
            }

            return false;
        }
    }
}
