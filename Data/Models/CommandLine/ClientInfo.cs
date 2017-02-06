using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using LNF.Scripting;

namespace Data.Models.CommandLine
{
    public class ClientInfoCollection : ModelCollection<ClientInfo>
    {
        public ClientInfoCollection(IEnumerable<ClientInfo> items)
            : base(items) { }

        public ClientInfo this[string username]
        {
            get { return items.First(x => x.UserName == username); }
        }
    }

    public class ClientInfo : ModelBase
    {
        public int ClientID { get; set; }
        public string UserName { get; set; }
        public string LName { get; set; }
        public string FName { get; set; }
        public string MName { get; set; }
        public bool Active { get; set; }

        public string DisplayName
        {
            get { return LName + ", " + FName; }
        }
    }
}