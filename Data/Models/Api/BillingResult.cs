using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Data.Models.Api
{
    public class BillingResult
    {
        public bool Success { get; set; }
        public string Command { get; set; }
        public string Description { get; set; }
        public string LogText { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double TimeTaken { get; set; }
    }
}