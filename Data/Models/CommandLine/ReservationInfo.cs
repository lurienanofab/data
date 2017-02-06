using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LNF.Scripting;

namespace Data.Models.CommandLine
{
    public class ReservationInfoCollection : ModelCollection<ReservationInfo>
    {
        public ReservationInfoCollection(IEnumerable<ReservationInfo> items)
            : base(items) { }
    }

    public class ReservationInfo : ModelBase
    {
        public int ReservationID { get; set; }
        public int ResourceID { get; set; }
        public int ClientID { get; set; }
        public int AccountID { get; set; }
        public int ActivityID { get; set; }
        public string ResourceName { get; set; }
        public string DisplayName { get; set; }
        public string AccountName { get; set; }
        public string ActivityName { get; set; }
        public DateTime BeginDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime ActualBeginDateTime { get; set; }
        public DateTime ActualEndDateTime { get; set; }
        public bool IsStarted { get; set; }
        public bool IsActive { get; set; }
    }
}