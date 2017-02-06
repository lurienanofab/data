using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LNF.Data;
using LNF.Repository;
using LNF.Repository.Data;

namespace Data.Models
{
    public class ClientListItem
    {
        public int ClientID { get; set; }
        public int ClientOrgID { get; set; }
        public int OrgID { get; set; }
        public string DisplayName { get; set; }
        public bool OrgManager { get; set; }
        public bool OrgFinManager { get; set; }
        public bool Active { get; set; }
        public bool HasDryBox { get; set; }
        public string DryBoxAccount { get; set; }
    }
}