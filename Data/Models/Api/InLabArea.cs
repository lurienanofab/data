using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Data.Models.Api
{
    public class InLabArea
    {
        public string AreaName { get; set; }
        public int Count { get { return Clients.Count; } }
        public IList<InLabClient> Clients { get; set; }
    }
}