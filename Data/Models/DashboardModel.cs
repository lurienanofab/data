using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LNF.Web.Mvc;

namespace Data.Models
{
    public class DashboardModel : BaseModel
    {
        public DateTime DefaultStartDate { get; set; }
        public DateTime DefaultEndDate { get; set; }
    }
}