using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Data.Models.Api
{
    public class BillingModel
    {
        public string Command { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public bool IsTemp { get; set; }
        public int ClientID { get; set; }
        public int ResourceID { get; set; }
        public int RoomID { get; set;}
        public int ItemID { get; set; }
    }
}