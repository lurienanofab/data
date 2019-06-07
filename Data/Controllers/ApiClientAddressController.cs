using Data.Models.Api;
using LNF.Repository;
using LNF.Repository.Data;
using System.Web.Http;

namespace Data.Controllers
{
    [Route("api/client/address/{option}")]
    public class ApiClientAddressController : ApiController
    {
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
            Address addr = DA.Current.Single<Address>(model.AddressID);
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
                DA.Current.SaveOrUpdate(addr);
                result = ApiUtility.CreateAddressModel("client", addr);
            }
            else
            {
                //add a new address to this ClientOrg
                ClientOrg co = DA.Current.Single<ClientOrg>(id);
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

                    DA.Current.SaveOrUpdate(addr);

                    co.ClientAddressID = addr.AddressID;

                    DA.Current.SaveOrUpdate(co);

                    result = ApiUtility.CreateAddressModel("client", addr);
                }
            }

            return result;
        }

        public bool Delete([FromBody] AddressModel model, int id)
        {
            Address addr = DA.Current.Single<Address>(model.AddressID);

            ClientOrg co = DA.Current.Single<ClientOrg>(id);

            if (addr != null && co != null)
            {
                DA.Current.Delete(addr);
                co.ClientAddressID = 0;
                DA.Current.SaveOrUpdate(co);

                return true;
            }

            return false;
        }
    }
}
