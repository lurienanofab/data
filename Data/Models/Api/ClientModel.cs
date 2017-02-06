using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Data.Models.Api
{
    public class ClientModel
    {
        public int ClientID { get; set; }
        public int ClientOrgID { get; set; }
        public int OrgID { get; set; }
        public string DisplayName { get; set; }
        public string OrgName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}