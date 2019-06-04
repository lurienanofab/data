using LNF.Models.Data;
using LNF.Web.Mvc;
using System;

namespace Data.Models
{
    public class DashboardModel : BaseModel
    {
        public DateTime DefaultStartDate { get; set; }
        public DateTime DefaultEndDate { get; set; }
    }
}