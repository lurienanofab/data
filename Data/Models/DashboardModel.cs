using LNF.Web.Mvc;
using System;

namespace Data.Models
{
    public class DashboardModel : BaseModel
    {
        public string CompanyName { get; set; }
        public DateTime DefaultStartDate { get; set; }
        public DateTime DefaultEndDate { get; set; }
    }
}