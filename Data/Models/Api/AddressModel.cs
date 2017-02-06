using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Data.Models.Api
{
    public enum OrgAddressType
    {
        Client = 1,
        Billing = 2,
        Shipping = 3
    }

    public class AddressModel
    {
        public int AddressID { get; set; }
        public string AddressType { get; set; }
        public string Attention { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
    }
}